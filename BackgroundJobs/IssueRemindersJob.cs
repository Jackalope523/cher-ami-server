using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CherAmiAPI.BackgroundJobs
{
    // Each send carries a deterministic idempotency key, so a same-day restart can't double-send.
    [DisallowConcurrentExecution]
    public class IssueRemindersJob(IServiceProvider _serviceProvider) : IJob
    {
        // The notifications API allows 10 requests a minute.
        private static readonly TimeSpan SendSpacing = TimeSpan.FromSeconds(7);

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            ApplicationDbContext ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            NotificationService notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

            DateTime today = DateTime.UtcNow.Date;

            var issues = await ctx.Issues
                .AsNoTracking()
                .Where(x => x.Status == IssueStatus.Drafting && x.DraftingEnd > DateTimeOffset.UtcNow)
                .Select(x => new
                {
                    x.Id,
                    x.CircleId,
                    x.DraftingEnd,
                    CircleTitle = x.Circle.Title,
                    Posts = x.Posts.Count,
                    Members = x.Circle.Contributors.Count,
                })
                .ToListAsync();

            Log.Information("IssueRemindersJob: {Count} issues in drafting.", issues.Count);

            foreach (var issue in issues)
            {
                // Tags only feed daily dashboard segments, so refreshing here keeps the fan-out out of requests.
                await notificationService.SyncCircleTagsAsync(issue.CircleId);

                int daysLeft = (int)(issue.DraftingEnd.UtcDateTime.Date - today).TotalDays;

                if (daysLeft is not (7 or 3 or 1)) continue;

                // A lone member with no photos hasn't started yet; they get activation nudges, not a deadline.
                if (issue.Posts >= NotificationService.PostsPerMagazine) continue;
                if (issue.Posts == 0 && issue.Members <= 1) continue;

                List<string> recipientNames = await ctx.Recipients
                    .AsNoTracking()
                    .Where(x => x.Manager.CircleId == issue.CircleId)
                    .Select(x => x.Name)
                    .ToListAsync();

                int remaining = NotificationService.PostsPerMagazine - issue.Posts;
                string month = issue.DraftingEnd.ToString("MMMM");
                string closeDate = issue.DraftingEnd.ToString("MMMM d");

                string heading;
                string content;
                string route = null;

                if (recipientNames.Count == 0)
                {
                    // No recipient: ask for one instead of photos. Skipped on the last day, where it reads as pressure.
                    if (daysLeft == 1) continue;

                    heading = $"Who should get {month}'s magazine?";
                    content = issue.Posts == 0
                        ? "Add an address and we'll mail it when the month ends."
                        : $"{issue.Posts} {(issue.Posts == 1 ? "photo" : "photos")} so far. Add an address and we'll mail it when the month ends.";
                    route = "/circle/recipients/add";
                }
                else
                {
                    string name = recipientNames.Count == 1 ? recipientNames[0] : issue.CircleTitle;

                    switch (daysLeft)
                    {
                        case 7:
                            if (issue.Posts >= NotificationService.PostsPerMagazine / 2) continue;

                            heading = $"{month}'s magazine";
                            content = issue.Posts == 0
                                ? $"There's room for {NotificationService.PostsPerMagazine} photos before it closes on {closeDate}."
                                : $"{issue.Posts} {(issue.Posts == 1 ? "photo" : "photos")} so far, and room for {remaining} more before it closes on {closeDate}.";
                            break;

                        case 3:
                            heading = $"{name}'s magazine closes {issue.DraftingEnd:dddd}";
                            content = $"There's room for {remaining} more {(remaining == 1 ? "photo" : "photos")} until then.";
                            break;

                        default:
                            heading = $"Last day for {month}'s magazine";
                            content = $"{name}'s copy goes to print tomorrow — there's room for {remaining} more {(remaining == 1 ? "photo" : "photos")}.";
                            break;
                    }
                }

                await notificationService.SendIssueReminderAsync(
                    issue.CircleId,
                    heading,
                    content,
                    route,
                    idempotencySeed: $"issue-reminder:{issue.Id}:{daysLeft}:{today:yyyy-MM-dd}");

                await Task.Delay(SendSpacing);
            }

            Log.Information("IssueRemindersJob complete.");
        }
    }
}
