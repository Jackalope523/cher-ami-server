using Core.Boundaries;
using CrazyLizard.Contexts;
using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Repository.Entities.Notification;

namespace CrazyLizard.Repositories
{
    public class NotificationRepository(CrazyLizardContext ctx) : INotificationRepository
    {
        public async Task<CoreNotificationProfile> GetNotificationProfileAsync(long userId)
        {
            return await
                ctx.Users.
                Where(u => u.Id == userId).
                Select(u => new CoreNotificationProfile(
                    u.Id, 
                    u.NotificationId, 
                    u.IssuePosts,
                    u.IssuePosts
                )).SingleAsync();
        }

        public async Task UpdateNotificationProfileAsync(long userId, List<(string Property, object Value)> edits)
        {
            User u = new() { Id = userId };

            ctx.Users.Attach(u);

            foreach ((string Property, object Value) in edits)
            {
                switch (Property)
                {
                    case nameof(CoreNotificationProfile.IssuePosts):
                        u.IssuePosts = (bool)Value;
                        break;
                    case nameof(CoreNotificationProfile.IssueReminders):
                        u.IssueReminders = (bool)Value;
                        break;
                    default:
                        throw new ArgumentException("Property named \"" + Property + "\" can not be updated using this method.");
                }
                ctx.Entry(u).Property(Property).IsModified = true;
            }
            await ctx.SaveChangesAsync();
        }

        public async Task ClearGatheringNotificationScheduleAsync(long gatheringId)
        {
            await ctx.Notifications.
            Where(n => n.GatheringId == gatheringId).
            ExecuteDeleteAsync();
        }
    }
}
