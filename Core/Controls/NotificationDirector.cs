using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Boundaries;
using Core.Entities;
using Core.Notifications;

using static Core.Entities.Psijic;
using static Core.Entities.Artificer;

namespace Core.Controls
{
    internal class NotificationDirector : AbstractDirector, INotificationOperations
	{
		#region Initialisation

		public NotificationDirector(CoreTerminal terminal) : base(terminal) { }

		#endregion

		#region Operations

		public async Task<NotificationPreferencesShard> GetNotificationPreferencesAsync(long userId)
		{
			var user = await GetUserAsync(userId);
			var profile = await user.NotificationProfile;

			return new(profile.NotificationId, profile.SocialInvitations, profile.CompanionActivity,
				profile.GatheringReminders, profile.GatheringActivity, profile.GatheringDiscovery);
		}

        public async Task UpdateNotificationPreferencesAsync(long userId,
            bool? socialInvitations = null, bool? companionActivity = null,
			bool? gatheringReminders = null, bool? gatheringActivity = null,
			bool? gatheringDiscovery = null)
		{
			var user = await GetUserAsync(userId);

            List<(string Property, object Value)> edits = new();

            if (IsNotNull(socialInvitations))
			{
                edits.Add((nameof(NotificationProfile.SocialInvitations), socialInvitations.Value));
            }
            if (IsNotNull(companionActivity))
			{
                edits.Add((nameof(NotificationProfile.CompanionActivity), companionActivity.Value));
            }
            if (IsNotNull(gatheringReminders))
			{
                edits.Add((nameof(NotificationProfile.GatheringReminders), gatheringReminders.Value));
            }
            if (IsNotNull(gatheringActivity))
			{
                edits.Add((nameof(NotificationProfile.GatheringActivity), gatheringActivity.Value));
            }
            if (IsNotNull(gatheringDiscovery))
			{
                edits.Add((nameof(NotificationProfile.GatheringDiscovery), gatheringDiscovery.Value));
            }

			await Notifications.UpdateNotificationProfileAsync(user.Id, edits);
		}

		#endregion

		#region Favours

		internal async Task<NotificationProfile> RequestNotificationProfileAsync(User user)
		{
			return await Notifications.GetNotificationProfileAsync(user.Id);
		}

		internal async Task<string> NotifyUserAsync(User user, CanaryNotification notification, DateTimeOffset? notifyAt = null)
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

		internal async Task<string> NotifyUsersAsync(CanaryNotification notification, DateTimeOffset? notifyAt = null, params User[] users)
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

		#endregion
	}
}
