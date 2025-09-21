using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Notifications;

namespace CrazyLizard.Interfaces
{
	public record CoreNotificationProfile(long UserId, Guid NotificationId,
		bool IssuePosts, bool IssueReminders);

    public interface INotificationRepository
    {
		Task<CoreNotificationProfile> GetNotificationProfileAsync(long userId);
        Task UpdateNotificationProfileAsync(long userId, List<(string Property, object Value)> edits);
	}

	public interface INotificationStorageService
	{
		Task<CoreNotificationProfile> GetNotificationPreferencesAsync(long userId);
		Task UpdateNotificationPreferencesAsync(long userId,
			bool? issuePosts = null, bool? issueReminders = null);
	}

	public interface INotificationService
	{
		Task<string> DispatchNotification(CardinalNotification notification, params CoreNotificationProfile[] notificationProfiles);
		Task<string> ScheduleNotification(CardinalNotification notification, DateTimeOffset dispatchAt, params CoreNotificationProfile[] notificationProfiles);
		Task CancelNotification(string notificationId);
	}
}
