using Core;

namespace Frontier.Controllers
{
	public class GuardBox
	{
		public EnvironmentOptions env;
		public ILogger log;

		public IAccountOperations accounts;
		public IChatOperations chat;
		public IConnectionOperations connections;
		public IGroupOperations groups;
		public IIssueOperations issues;
		public IKeyOperations keys;
		public IMediaOperations media;
        public IMiscellaneousOperations miscellaneous;
		public INotificationOperations notifications;
		public IProfileOperations profiles;
		public IReportOperations reports;

        public GuardBox(EnvironmentOptions environment, ILogger logger,
			IAccountOperations accountOperations, IConnectionOperations connectionOperations,
			IProfileOperations profileOperations, IGroupOperations groupOperations,
			IIssueOperations issueOperations, IKeyOperations keyOperations,
			IReportOperations reportOperations,IMediaOperations mediaOperations,
			IChatOperations chatOperations,
			INotificationOperations notificationOperations, IMiscellaneousOperations miscellaneousOperations)
		{
			env = environment;
			log = logger;

			accounts = accountOperations;
			chat = chatOperations;
			connections = connectionOperations;
			groups = groupOperations;
			issues = issueOperations;
			keys = keyOperations;
			media = mediaOperations;
			miscellaneous = miscellaneousOperations;
			notifications = notificationOperations;
			profiles = profileOperations;
			reports = reportOperations;
		}
	}
}
