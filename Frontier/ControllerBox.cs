using Core;

namespace Frontier.Controllers
{
	public class ControllerBox
	{
		public EnvironmentOptions env;
		public ILogger log;

		public IAccountOperations accounts;
		public ICircleOperations circles;
		public IIssueOperations issues;
		public IKeyOperations keys;
		public IMediaOperations media;
        public IMiscellaneousOperations miscellaneous;
		public INotificationOperations notifications;
		public IOrderOperations orders;
		public IProfileOperations profiles;
		public IReportOperations reports;

        public ControllerBox(EnvironmentOptions environment, ILogger logger,
			IAccountOperations accountOperations,
			IProfileOperations profileOperations, ICircleOperations circleOperations,
			IIssueOperations issueOperations, IKeyOperations keyOperations,
			IReportOperations reportOperations,IMediaOperations mediaOperations,
			INotificationOperations notificationOperations,
			IOrderOperations orderOperations,
			IMiscellaneousOperations miscellaneousOperations)
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
