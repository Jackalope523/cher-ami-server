using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Boundaries;
using Core.Entities;
using Core.Notifications;
using static Core.Entities.Artificer;

namespace Core.Services
{
    public class NotificationStorageService(INotificationRepository notificationRepository, IAccountRepository accountRepository, INotificationService notificationService) : INotificationStorageService
	{
		public async Task<NotificationPreferencesShard> GetNotificationPreferencesAsync(long userId)
		{
			NotificationProfile profile = await notificationRepository.GetNotificationProfileAsync(userId);
			return new(profile.NotificationId, profile.IssuePosts, profile.IssueReminders);
		}

        public async Task UpdateNotificationPreferencesAsync(long userId, bool? issuePosts = null, bool? issueReminders = null)
        {
            CoreUser user = await accountRepository.GetUserByIdAsync(userId);

            List<(string Property, object Value)> edits = new();

            if (IsNotNull(issuePosts))
            {
                edits.Add((nameof(NotificationProfile.IssuePosts), issuePosts.Value));
            }
            if (IsNotNull(issueReminders))
            {
                edits.Add((nameof(NotificationProfile.IssueReminders), issueReminders.Value));
            }

            await notificationRepository.UpdateNotificationProfileAsync(user.Id, edits);
        }

		internal async Task<NotificationProfile> RequestNotificationProfileAsync(User user)
		{
			return await notificationRepository.GetNotificationProfileAsync(user.Id);
		}

		internal async Task<string> NotifyUserAsync(User user, CardinalNotification notification, DateTimeOffset? notifyAt = null)
		{
			string notificationId;

			if (IsNotNull(notifyAt))
			{
                notificationId = await notificationService.ScheduleNotification(notification, notifyAt.Value, user.NotificationProfile);
			}
			else
			{
				notificationId = await notificationService.DispatchNotification(notification, user.NotificationProfile);
            }

			return notificationId;
        }

		internal async Task<string> NotifyUsersAsync(CardinalNotification notification, DateTimeOffset? notifyAt = null, params User[] users)
		{
			var profiles = users.Select(user => user.NotificationProfile).ToArray();

			string notificationId;

			if (IsNotNull(notifyAt))
			{
				notificationId = await notificationService.ScheduleNotification(notification, notifyAt.Value, profiles);
			}
			else
			{
				notificationId = await notificationService.DispatchNotification(notification, profiles);
			}

			return notificationId;
		}

		internal async Task CancelScheduledNotifications(params string[] notificationIds)
		{
			foreach (var id in notificationIds)
			{
				await notificationService.CancelNotification(id);
			}
		}
    }
}
