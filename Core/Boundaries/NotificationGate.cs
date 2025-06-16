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
		bool SocialInvitations, bool CompanionActivity,
		bool GatheringReminders, bool GatheringActivity,
		bool GatheringDiscovery)
		: CoreOnlyData();

	public record NotificationPreferencesShard(Guid NotificationId,
		bool SocialInvitations, bool CompanionActivity,
		bool GatheringReminders, bool GatheringActivity,
		bool GatheringDiscovery);

	public record HostNotificationSchedule(string GatheringWaitingId);

	public record GuestNotificationSchedule(long UserId,
		string GatheringUpcomingId, string GatheringImminentId);

    #endregion

    #region Gates

    public interface INotificationDatabase
    {
		Task<NotificationProfile> GetNotificationProfileAsync(long userId);
        Task UpdateNotificationProfileAsync(long userId, List<(string Property, object Value)> edits);

		Task<(HostNotificationSchedule HostSchedule, List<GuestNotificationSchedule> GuestSchedules)> GetGatheringNotificationScheduleAsync(long gatheringId);
		Task UpdateGatheringHostNotificationScheduleAsync(long gatheringId, string gatheringWaitingId);
		Task UpdateGatheringGuestNotificationSchedulesAsync(long gatheringId, params (long userId, string gatheringUpcomingId, string gatheringImminentId)[] guestSchedules);
		Task ClearGatheringNotificationScheduleAsync(long gatheringId);
	}

	public interface INotificationOperations
	{
		Task<NotificationPreferencesShard> GetNotificationPreferencesAsync(long userId);
		Task UpdateNotificationPreferencesAsync(long userId,
			bool? socialInvitations = null, bool? companionActivity = null,
			bool? gatheringReminders = null, bool? gatheringActivity = null,
			bool? gatheringDiscovery = null);
	}

	public interface INotificationService
	{
		Task<string> DispatchNotification(CanaryNotification notification, params NotificationProfile[] notificationProfiles);
		Task<string> ScheduleNotification(CanaryNotification notification, DateTimeOffset dispatchAt, params NotificationProfile[] notificationProfiles);
		Task CancelNotification(string notificationId);
	}

	#endregion
}
