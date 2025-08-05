using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Boundaries;
using static LazyLizardBackend.Artificer;

namespace Core.Services
{
    public class NotificationStorageService(INotificationRepository notificationRepository, IAccountRepository accountRepository) : INotificationStorageService
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
    }
}
