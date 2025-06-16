using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using static Repository.Notification;

namespace Repository
{
    public class EFCoreNotificationStore : QueryStore, INotificationDatabase
    {
        public EFCoreNotificationStore(Harbor.Flag flag) : base(flag)
        {
        }

        public async Task DeleteTelegramAsync(long telegramId)
        {
            await storeSentry.ExecuteWriteAsync(ctx => ctx.Telegrams.Remove(new Telegram { Id = telegramId }));
        }

        public async Task<NotificationProfile> GetNotificationProfileAsync(long userId)
        {
            return await storeSentry.ExecuteReadAsync(ctx => 
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
                )).SingleAsync());
        }

        public async Task UpdateNotificationProfileAsync(long userId, List<(string Property, object Value)> edits)
        {
            Discussion currentDiscussion = storeSentry.BeginDiscussion();

            User u = new() { Id = userId };

            storeSentry.DiscussWrite(ctx => ctx.Users.Attach(u), currentDiscussion);

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
                storeSentry.DiscussWrite(ctx => ctx.Entry(u).Property(Property).IsModified = true, currentDiscussion);
            }
            await storeSentry.EndDiscussionAsync(currentDiscussion);
        }

        public async Task ClearGatheringNotificationScheduleAsync(long gatheringId)
        {
            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Notifications.
                Where(n => n.GatheringId == gatheringId).
                ExecuteDeleteAsync());
        }

        public async Task<(HostNotificationSchedule, List<GuestNotificationSchedule>)> GetGatheringNotificationScheduleAsync(long gatheringId)
        {
            List<Notification> notifications = await storeSentry.ExecuteReadAsync(ctx =>
                                                ctx.Notifications.
                                                Where(n => n.GatheringId == gatheringId).
                                                ToListAsync());

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
            Discussion discussion = storeSentry.BeginDiscussion();

            await storeSentry.DiscussWriteAsync(ctx => 
                ctx.Notifications.
                Where(n => n.GatheringId == gatheringId && n.Type != NotificationType.GatheringWaiting).
                ExecuteDeleteAsync(), discussion);

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

                storeSentry.DiscussWrite(ctx => ctx.Notifications.AddRange(upcoming, imminent), discussion);
            }

            await storeSentry.EndDiscussionAsync(discussion);
        }

        public async Task UpdateGatheringHostNotificationScheduleAsync(long gatheringId, string gatheringWaitingId)
        {
            Discussion discussion = storeSentry.BeginDiscussion();

            await storeSentry.DiscussWriteAsync(ctx =>
               ctx.Notifications.
               Where(n => n.GatheringId == gatheringId && n.Type == NotificationType.GatheringWaiting).
               ExecuteDeleteAsync(), discussion);

            long? hostId = await storeSentry.ExecuteReadAsync(ctx => 
                            ctx.Gatherings.
                            Where(g => g.Id == gatheringId).
                            Select(g => g.HostId).
                            SingleAsync());

            storeSentry.DiscussWrite(ctx =>
               ctx.Notifications.
               Add(new() 
               { 
                   RecipientId = hostId ?? 0,
                   GatheringId = gatheringId, 
                   NotificationId = gatheringWaitingId, 
                   Type = NotificationType.GatheringWaiting
                }
            ), discussion);

            storeSentry.EndDiscussion(discussion);
        }
    }
}
