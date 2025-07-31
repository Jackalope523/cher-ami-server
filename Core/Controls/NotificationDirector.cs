using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Boundaries;
using Core.Entities;
using Core.Notifications;
using static Core.Entities.Artificer;

namespace Core.Controls
{
    internal class NotificationDirector : AbstractDirector, INotificationOperations
	{
		public NotificationDirector(CoreTerminal terminal) : base(terminal) { }

		public async Task<NotificationPreferencesShard> GetNotificationPreferencesAsync(long userId)
		{
			var user = await GetUserAsync(userId);
			var profile = await user.NotificationProfile;

			return new(profile.NotificationId, profile.IssuePosts, profile.IssueReminders);
		}

        public async Task UpdateNotificationPreferencesAsync(long userId, bool? issuePosts = null, bool? issueReminders = null)
        {
            var user = await GetUserAsync(userId);

            List<(string Property, object Value)> edits = new();

            if (IsNotNull(issuePosts))
            {
                edits.Add((nameof(NotificationProfile.IssuePosts), issuePosts.Value));
            }
            if (IsNotNull(issueReminders))
            {
                edits.Add((nameof(NotificationProfile.IssueReminders), issueReminders.Value));
            }

            await Notifications.UpdateNotificationProfileAsync(user.Id, edits);
        }

		internal async Task<NotificationProfile> RequestNotificationProfileAsync(User user)
		{
			return await Notifications.GetNotificationProfileAsync(user.Id);
		}

		internal async Task<string> NotifyUserAsync(User user, CardinalNotification notification, DateTimeOffset? notifyAt = null)
		{
			string notificationId;

			if (IsNotNull(notifyAt))
			{
                notificationId = await Terminal.NotificationService.ScheduleNotification(notification, notifyAt.Value, await user.NotificationProfile);
			}
			else
			{
				notificationId = await Terminal.NotificationService.DispatchNotification(notification, await user.NotificationProfile);
            }

			return notificationId;
        }

		internal async Task<string> NotifyUsersAsync(CardinalNotification notification, DateTimeOffset? notifyAt = null, params User[] users)
		{
			var profiles = await Task.WhenAll(users.Select(user => user.NotificationProfile.Value()));

			string notificationId;

			if (IsNotNull(notifyAt))
			{
				notificationId = await Terminal.NotificationService.ScheduleNotification(notification, notifyAt.Value, profiles);
			}
			else
			{
				notificationId = await Terminal.NotificationService.DispatchNotification(notification, profiles);
			}

			return notificationId;
		}

		internal async Task CancelScheduledNotifications(params string[] notificationIds)
		{
			foreach (var id in notificationIds)
			{
				await Terminal.NotificationService.CancelNotification(id);
			}
		}
    }
}
