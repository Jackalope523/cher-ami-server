using System.Collections.Generic;
using System.Threading.Tasks;

namespace LazyLizardBackend.Services
{
    public class NotificationStorageService(INotificationRepository notificationRepository, IAccountRepository accountRepository) : INotificationStorageService
	{
		public async Task<CoreNotificationProfile> GetNotificationPreferencesAsync(long userId)
		{
			return await notificationRepository.GetNotificationProfileAsync(userId);
		}

        public async Task UpdateNotificationPreferencesAsync(long userId, bool? issuePosts = null, bool? issueReminders = null)
        {
            CoreUser user = await accountRepository.GetUserByIdAsync(userId);

            List<(string Property, object Value)> edits = new();

            if (issuePosts != null)
            {
                edits.Add((nameof(CoreNotificationProfile.IssuePosts), issuePosts.Value));
            }
            if (issueReminders != null)
            {
                edits.Add((nameof(CoreNotificationProfile.IssueReminders), issueReminders.Value));
            }

            await notificationRepository.UpdateNotificationProfileAsync(user.Id, edits);
        }
    }
}
