using Microsoft.EntityFrameworkCore;
using Repository.Databases.Contexts;
using Repository.Databases.Entities;
using static Repository.Databases.Entities.Notification;

namespace Repository.Databases.Stores
{
    public class NotificationRepository : Repository, INotificationDatabase
    {
        internal NotificationRepository(Func<CanaryContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task<NotificationProfile> GetNotificationProfileAsync(long userId)
        {
            await using CanaryContext ctx = initContext();

            return await
                ctx.Users.
                Where(u => u.Id == userId).
                Select(u => new NotificationProfile(
                    u.Id, 
                    u.NotificationId, 
                    u.SocialInvitations,
                    u.CompanionActivity,
                    u.GatheringReminders, 
                    u.GatheringActivity, 
                    u.GatheringDiscovery
                )).SingleAsync();
        }

        public async Task UpdateNotificationProfileAsync(long userId, List<(string Property, object Value)> edits)
        {
            await using CanaryContext ctx = initContext();

            User u = new() { Id = userId };

            ctx.Users.Attach(u);

            foreach ((string Property, object Value) in edits)
            {
                switch (Property)
                {
                    case nameof(NotificationProfile.SocialInvitations):
                        u.SocialInvitations = (bool)Value;
                        break;
                    case nameof(NotificationProfile.CompanionActivity):
                        u.CompanionActivity = (bool)Value;
                        break;
                    case nameof(NotificationProfile.GatheringReminders):
                        u.GatheringReminders = (bool)Value;
                        break;
                    case nameof(NotificationProfile.GatheringActivity):
                        u.GatheringActivity = (bool)Value;
                        break;
                    case nameof(NotificationProfile.GatheringDiscovery):
                        u.GatheringDiscovery = (bool)Value;
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
            await using CanaryContext ctx = initContext();

            await ctx.Notifications.
            Where(n => n.GatheringId == gatheringId).
            ExecuteDeleteAsync();
        }

        public async Task<(HostNotificationSchedule, List<GuestNotificationSchedule>)> GetGatheringNotificationScheduleAsync(long gatheringId)
        {
            List<Notification> notifications;

            await using (CanaryContext ctx = initContext())
            {
                notifications = await ctx.Notifications.
                                Where(n => n.GatheringId == gatheringId).
                                ToListAsync();
            }

            HostNotificationSchedule hostNotification = new("ERROR");
            Dictionary<long, string> guestUpcomingNotifications = new();
            Dictionary<long, string> guestImminentNotifications = new();

            foreach (Notification notification in notifications) 
            {
                switch (notification.Type)
                {
                    case NotificationType.GatheringWaiting:
                        hostNotification = new(notification.NotificationId);
                        break;
                    case NotificationType.GatheringUpcoming:
                        guestUpcomingNotifications.Add(notification.RecipientId, notification.NotificationId);
                        break;
                    case NotificationType.GatheringImminent:
                        guestImminentNotifications.Add(notification.RecipientId, notification.NotificationId);
                        break;
                    default:
                        break;
                }
            }

            List<GuestNotificationSchedule> guestNotifications = new();
            foreach (long id in guestUpcomingNotifications.Keys)
            {
                guestNotifications.Add(new(id, guestUpcomingNotifications[id], guestImminentNotifications[id]));
            }

            return (hostNotification, guestNotifications);
        }

        public async Task UpdateGatheringGuestNotificationSchedulesAsync(long gatheringId, params (long userId, string gatheringUpcomingId, string gatheringImminentId)[] guestSchedules)
        {
            await using CanaryContext ctx = initContext();
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                await ctx.Notifications.
                Where(n => n.GatheringId == gatheringId && n.Type != NotificationType.GatheringWaiting).
                ExecuteDeleteAsync();

                foreach (var schedule in guestSchedules)
                {
                    Notification upcoming = new()
                    {
                        GatheringId = gatheringId,
                        RecipientId = schedule.userId,
                        NotificationId = schedule.gatheringUpcomingId,
                        Type = NotificationType.GatheringUpcoming,
                    };

                    Notification imminent = new()
                    {
                        GatheringId = gatheringId,
                        RecipientId = schedule.userId,
                        NotificationId = schedule.gatheringImminentId,
                        Type = NotificationType.GatheringImminent,
                    };

                    ctx.Notifications.AddRange(upcoming, imminent);
                }

                await ctx.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateGatheringHostNotificationScheduleAsync(long gatheringId, string gatheringWaitingId)
        {
            await using CanaryContext ctx = initContext();
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                await ctx.Notifications.
                Where(n => n.GatheringId == gatheringId && n.Type == NotificationType.GatheringWaiting).
                ExecuteDeleteAsync();

                long? hostId = await ctx.Gatherings.
                               Where(g => g.Id == gatheringId).
                               Select(g => g.HostId).
                               SingleAsync();

                ctx.Notifications.
                Add(new()
                {
                    RecipientId = hostId ?? 0,
                    GatheringId = gatheringId,
                    NotificationId = gatheringWaitingId,
                    Type = NotificationType.GatheringWaiting
                }
                );

                await ctx.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
