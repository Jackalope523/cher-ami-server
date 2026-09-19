using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Services
{
    // Every method swallows its own failures: OneSignal being down must never fail a user's action.
    public class NotificationService(ApplicationDbContext ctx, OneSignalService oneSignalService)
    {
        public const int PostsPerMagazine = 20;

        // Enums rather than raw counts: the OneSignal plan allows only 10 tags per user.
        public async Task SyncTagsAsync(long userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await ctx.Users
                    .AsNoTracking()
                    .Where(x => x.Id == userId)
                    .Select(x => new { x.ExternalId, x.CircleId })
                    .SingleOrDefaultAsync(cancellationToken);

                if (user == null || user.ExternalId == default) return;

                Dictionary<string, string> tags = new()
                {
                    ["lifecycle_stage"] = await LifecycleStageAsync(user.CircleId, cancellationToken),
                };

                foreach (var tag in await CurrentIssueTagsAsync(user.CircleId, cancellationToken))
                {
                    tags[tag.Key] = tag.Value;
                }

                await oneSignalService.SetTagsAsync(user.ExternalId, tags, cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to sync OneSignal tags for user {UserId}", userId);
            }
        }

        private async Task<string> LifecycleStageAsync(long? circleId, CancellationToken cancellationToken)
        {
            if (!circleId.HasValue) return "no_circle";

            if (await ctx.Users.CountAsync(x => x.CircleId == circleId, cancellationToken) <= 1)
                return "alone";

            if (!await ctx.Recipients.AnyAsync(x => x.Manager.CircleId == circleId, cancellationToken))
                return "no_recipient";

            return "active";
        }

        public async Task SyncCircleTagsAsync(long circleId, CancellationToken cancellationToken = default)
        {
            try
            {
                List<long> memberIds = await ctx.Users
                    .AsNoTracking()
                    .Where(x => x.CircleId == circleId)
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken);

                foreach (long memberId in memberIds)
                {
                    await SyncTagsAsync(memberId, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to sync OneSignal tags for circle {CircleId}", circleId);
            }
        }

        // Kept out of SyncTagsAsync: the database doesn't mirror Stripe, so a sync would clear a failed payment.
        public async Task SetSubscriptionStatusAsync(long userId, string status, CancellationToken cancellationToken = default)
        {
            try
            {
                Guid externalId = await ctx.Users
                    .AsNoTracking()
                    .Where(x => x.Id == userId)
                    .Select(x => x.ExternalId)
                    .SingleOrDefaultAsync(cancellationToken);

                if (externalId == default) return;

                await oneSignalService.AddTagAsync(externalId, "subscription_status", status, cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to set subscription_status for user {UserId}", userId);
            }
        }

        public async Task SyncSubscriptionStatusAsync(long userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await ctx.Users
                    .AsNoTracking()
                    .Where(x => x.Id == userId)
                    .Select(x => new { x.StripeSubscriptionId })
                    .SingleOrDefaultAsync(cancellationToken);

                if (user == null) return;

                bool hasRecipient = await ctx.Recipients.AnyAsync(x => x.ManagerId == userId, cancellationToken);

                string status = !hasRecipient ? "none"
                              : string.IsNullOrEmpty(user.StripeSubscriptionId) ? "free_first"
                              : "active";

                await SetSubscriptionStatusAsync(userId, status, cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to sync subscription_status for user {UserId}", userId);
            }
        }

        public async Task SyncEmailPreferencesAsync(long userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await ctx.Users
                    .AsNoTracking()
                    .Where(x => x.Id == userId)
                    .Select(x => new { x.ExternalId, x.EmailIssueReminders, x.EmailMarketing })
                    .SingleOrDefaultAsync(cancellationToken);

                if (user == null || user.ExternalId == default) return;

                await oneSignalService.SetTagsAsync(user.ExternalId, new Dictionary<string, string>
                {
                    ["email_reminders"] = user.EmailIssueReminders ? "1" : "0",
                    ["email_marketing"] = user.EmailMarketing ? "1" : "0",
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to sync email preferences for user {UserId}", userId);
            }
        }

        public async Task MarkPostedAsync(long userId, CancellationToken cancellationToken = default)
        {
            try
            {
                Guid externalId = await ctx.Users
                    .AsNoTracking()
                    .Where(x => x.Id == userId)
                    .Select(x => x.ExternalId)
                    .SingleOrDefaultAsync(cancellationToken);

                if (externalId == default) return;

                await oneSignalService.AddTagAsync(externalId, "last_posted_at", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to set last_posted_at for user {UserId}", userId);
            }
        }

        public async Task SendIssueReminderAsync(
            long circleId,
            string heading,
            string content,
            string route,
            string idempotencySeed,
            CancellationToken cancellationToken = default)
        {
            try
            {
                List<Guid> externalIds = await ctx.Users
                    .AsNoTracking()
                    .Where(x => x.CircleId == circleId && x.PushIssueReminders && x.ExternalId != default)
                    .Select(x => x.ExternalId)
                    .ToListAsync(cancellationToken);

                if (externalIds.Count == 0) return;

                await oneSignalService.SendPushAsync(
                    externalIds,
                    heading,
                    content,
                    data: route == null ? null : new Dictionary<string, string> { ["route"] = route },
                    idempotencyKey: OneSignalService.IdempotencyKeyFor(idempotencySeed),
                    cancellationToken: cancellationToken);

                Log.Information("Sent issue reminder to circle {CircleId}: {Heading}", circleId, heading);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send issue reminder to circle {CircleId}", circleId);
            }
        }

        public async Task SendPhotoActivityAsync(
            long circleId,
            List<long> authorIds,
            string heading,
            string content,
            string idempotencySeed,
            CancellationToken cancellationToken = default)
        {
            try
            {
                List<Guid> externalIds = await ctx.Users
                    .AsNoTracking()
                    .Where(x => x.CircleId == circleId && x.PushNewPosts && x.ExternalId != default && !authorIds.Contains(x.Id))
                    .Select(x => x.ExternalId)
                    .ToListAsync(cancellationToken);

                if (externalIds.Count == 0) return;

                await oneSignalService.SendPushAsync(
                    externalIds,
                    heading,
                    content,
                    data: new Dictionary<string, string> { ["route"] = "/feed" },
                    idempotencyKey: OneSignalService.IdempotencyKeyFor(idempotencySeed),
                    cancellationToken: cancellationToken);

                Log.Information("Sent photo activity to circle {CircleId}: {Heading}", circleId, heading);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send photo activity to circle {CircleId}", circleId);
            }
        }

        public async Task SendNewMemberAsync(long circleId, long joinedUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                string firstName = await ctx.Users
                    .AsNoTracking()
                    .Where(x => x.Id == joinedUserId)
                    .Select(x => x.FirstName)
                    .SingleOrDefaultAsync(cancellationToken);

                string circleTitle = await ctx.Circles
                    .AsNoTracking()
                    .Where(x => x.Id == circleId)
                    .Select(x => x.Title)
                    .SingleOrDefaultAsync(cancellationToken);

                List<Guid> externalIds = await ctx.Users
                    .AsNoTracking()
                    .Where(x => x.CircleId == circleId && x.Id != joinedUserId && x.PushNewMembers && x.ExternalId != default)
                    .Select(x => x.ExternalId)
                    .ToListAsync(cancellationToken);

                if (externalIds.Count == 0) return;

                string name = string.IsNullOrWhiteSpace(firstName) ? "Someone new" : firstName;
                string circle = string.IsNullOrWhiteSpace(circleTitle) ? "the family" : circleTitle;

                await oneSignalService.SendPushAsync(
                    externalIds,
                    $"{name} joined {circle}!",
                    "Say hello, and see what they add to this month's magazine.",
                    data: new Dictionary<string, string> { ["route"] = "/manage" },
                    idempotencyKey: OneSignalService.IdempotencyKeyFor($"member-joined:{circleId}:{joinedUserId}"),
                    cancellationToken: cancellationToken);

                Log.Information("Sent new member push to circle {CircleId} for user {UserId}", circleId, joinedUserId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send new member push to circle {CircleId}", circleId);
            }
        }

        private async Task<Dictionary<string, string>> CurrentIssueTagsAsync(long? circleId, CancellationToken cancellationToken)
        {
            Dictionary<string, string> none = new() { ["magazine_state"] = "empty", ["issue_close_at"] = "" };

            if (!circleId.HasValue) return none;

            var issue = await ctx.Issues
                .AsNoTracking()
                .Where(x => x.CircleId == circleId && x.Status == IssueStatus.Drafting)
                .OrderBy(x => x.DraftingEnd)
                .Select(x => new { x.Id, x.DraftingEnd })
                .FirstOrDefaultAsync(cancellationToken);

            if (issue == null) return none;

            // The magazine is a circle-level artifact with a circle-level cap, so this
            // counts the circle's photos, not the user's.
            int posts = await ctx.Posts.CountAsync(x => x.IssueId == issue.Id, cancellationToken);

            return new Dictionary<string, string>
            {
                ["magazine_state"] = posts == 0 ? "empty"
                                   : posts >= PostsPerMagazine ? "full"
                                   : posts >= PostsPerMagazine / 2 ? "nearly_full"
                                   : "filling",
                ["issue_close_at"] = issue.DraftingEnd.ToUnixTimeSeconds().ToString(),
            };
        }
    }
}
