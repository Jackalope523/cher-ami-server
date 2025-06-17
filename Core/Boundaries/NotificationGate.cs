using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Notifications;

namespace Core.Boundaries
{
    #region Schemas

	public record NotificationProfile(long UserId, Guid NotificationId,
		bool SegmentPosts, bool PostReminders)
		: CoreOnlyData();

	public record NotificationPreferencesShard(Guid NotificationId,
		bool SegmentPosts, bool PostReminders);

    #endregion

    #region Gates

    public interface INotificationDatabase
    {
		Task<NotificationProfile> GetNotificationProfileAsync(long userId);
        Task UpdateNotificationProfileAsync(long userId, List<(string Property, object Value)> edits);
	}

	public interface INotificationOperations
	{
		Task<NotificationPreferencesShard> GetNotificationPreferencesAsync(long userId);
		Task UpdateNotificationPreferencesAsync(long userId,
			bool? segmentPosts = null, bool? postReminders = null);
	}

	public interface INotificationService
	{
		Task<string> DispatchNotification(CardinalNotification notification, params NotificationProfile[] notificationProfiles);
		Task<string> ScheduleNotification(CardinalNotification notification, DateTimeOffset dispatchAt, params NotificationProfile[] notificationProfiles);
		Task CancelNotification(string notificationId);
	}

	#endregion
}
