using Core;

namespace Frontier.Controllers
{
	public class ControllerBox
	{
		public EnvironmentOptions env;
		public ILogger log;

		public IAccountService accounts;
		public ICircleService circles;
		public IIssueService issues;
		public IKeyService keys;
		public IMediaService media;
        public IMiscellaneousService miscellaneous;
		public INotificationStorageService notifications;
		public IOrderOperations orders;
		public IProfileService profiles;
		public IReportService reports;

        public ControllerBox(EnvironmentOptions environment, ILogger logger,
			IAccountService accountOperations,
			IProfileService profileOperations, ICircleService circleOperations,
			IIssueService issueOperations, IKeyService keyOperations,
			IReportService reportOperations,IMediaService mediaOperations,
			INotificationStorageService notificationOperations,
			IOrderOperations orderOperations,
			IMiscellaneousService miscellaneousOperations)
		{
			env = environment;
			log = logger;

			accounts = accountOperations;
			circles = circleOperations;
			issues = issueOperations;
			keys = keyOperations;
			media = mediaOperations;
			miscellaneous = miscellaneousOperations;
			notifications = notificationOperations;
			orders = orderOperations;
			profiles = profileOperations;
			reports = reportOperations;
		}
	}
}
