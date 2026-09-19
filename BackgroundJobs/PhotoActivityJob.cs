using CherAmiAPI.Contexts;
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
    [DisallowConcurrentExecution]
    public class PhotoActivityJob(IServiceProvider _serviceProvider) : IJob
    {
        private static readonly TimeSpan PushCooldown = TimeSpan.FromHours(4);

        // The notifications API allows 10 requests a minute.
        private static readonly TimeSpan SendSpacing = TimeSpan.FromSeconds(7);

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            ApplicationDbContext ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            NotificationService notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

            DateTimeOffset cooldownFloor = DateTimeOffset.UtcNow - PushCooldown;

            var circles = await ctx.Circles
                .AsNoTracking()
                .Where(x => x.Contributors.Count > 1 && (x.LastPhotoPushAt == null || x.LastPhotoPushAt <= cooldownFloor))
                .Select(x => new { x.Id, x.LastPhotoPushAt })
                .ToListAsync();

            Log.Information("PhotoActivityJob: {Count} circles off cooldown.", circles.Count);

            foreach (var circle in circles)
            {
                // A circle that has never been pushed starts at the cooldown floor, so switching
                // this job on doesn't announce every photo the family has ever added.
                DateTimeOffset since = circle.LastPhotoPushAt ?? cooldownFloor;

                var posts = await ctx.Posts
                    .AsNoTracking()
                    .Where(x => x.Issue.CircleId == circle.Id && x.PostedAt > since)
                    .OrderBy(x => x.PostedAt)
                    .Select(x => new { x.AuthorId, AuthorName = x.Author.FirstName, x.PostedAt, Month = x.Issue.DraftingEnd })
                    .ToListAsync();

                if (posts.Count == 0) continue;

                List<long> authorIds = posts.Select(x => x.AuthorId).Distinct().ToList();
                List<string> names = posts
                    .GroupBy(x => x.AuthorId)
                    .Select(x => x.First().AuthorName)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                string who = names.Count switch
                {
                    0 => "The family",
                    1 => names[0],
                    2 => $"{names[0]} and {names[1]}",
                    _ => $"{names[0]} and {names.Count - 1} others",
                };

                string photos = posts.Count == 1 ? "a photo" : $"{posts.Count} photos";
                string month = posts[^1].Month.ToString("MMMM");

                await notificationService.SendPhotoActivityAsync(
                    circle.Id,
                    authorIds,
                    $"{who} added {photos}!",
                    $"Go and see what's new in {month}'s magazine.",
                    idempotencySeed: $"photo-activity:{circle.Id}:{posts[^1].PostedAt.UtcTicks}");

                // The watermark is the newest photo announced, not the send time, so a photo
                // finalized while this job runs is picked up next time instead of being skipped.
                DateTimeOffset watermark = posts[^1].PostedAt;

                await ctx.Circles
                    .Where(x => x.Id == circle.Id)
                    .ExecuteUpdateAsync(x => x.SetProperty(c => c.LastPhotoPushAt, watermark));

                await Task.Delay(SendSpacing);
            }

            Log.Information("PhotoActivityJob complete.");
        }
    }
}
