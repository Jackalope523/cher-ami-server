using Microsoft.EntityFrameworkCore;
using Repository.Contexts;
using Repository.Entities;
using static Repository.Entities.Notification;

namespace Repository.Repositories
{
    public class NotificationRepository : Repository, INotificationRepository
    {
        internal NotificationRepository(Func<LLContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task<NotificationProfile> GetNotificationProfileAsync(long userId)
        {
            await using LLContext ctx = initContext();

            return await
                ctx.Users.
                Where(u => u.Id == userId).
                Select(u => new NotificationProfile(
                    u.Id, 
                    u.NotificationId, 
                    u.IssuePosts,
                    u.IssuePosts
                )).SingleAsync();
        }

        public async Task UpdateNotificationProfileAsync(long userId, List<(string Property, object Value)> edits)
        {
            await using LLContext ctx = initContext();

            User u = new() { Id = userId };

            ctx.Users.Attach(u);

            foreach ((string Property, object Value) in edits)
            {
                switch (Property)
                {
                    case nameof(NotificationProfile.IssuePosts):
                        u.IssuePosts = (bool)Value;
                        break;
                    case nameof(NotificationProfile.IssueReminders):
                        u.IssueReminders = (bool)Value;
                        break;
                    default:
                        throw new InvalidInputException("Property named \"" + Property + "\" can not be updated using this method.");
                }
                ctx.Entry(u).Property(Property).IsModified = true;
            }
            await ctx.SaveChangesAsync();
        }

        public async Task ClearGatheringNotificationScheduleAsync(long gatheringId)
        {
            await using LLContext ctx = initContext();

            await ctx.Notifications.
            Where(n => n.GatheringId == gatheringId).
            ExecuteDeleteAsync();
        }
    }
}
