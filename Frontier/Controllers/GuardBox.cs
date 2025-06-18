using Core;

namespace Frontier.Controllers
{
	public class GuardBox
	{
		public EnvironmentOptions env;
		public ILogger log;

		public IAccountOperations accounts;
		public IConnectionOperations connections;
		public IGroupOperations gatherings;
		public IIssueOperations snapshots;
		public IKeyOperations keys;
		public IDisciplineOperations reports;
		public IMediaOperations media;
		public IChatOperations messages;
		public IProfileOperations nests;
		public INotificationOperations notifications;
        public IMiscellaneousOperations miscellaneous;

        public GuardBox(EnvironmentOptions environment, ILogger logger,
			IAccountOperations accountOperations, IConnectionOperations connectionOperations,
			IProfileOperations nestOperations, IGroupOperations gatheringOperations,
			IIssueOperations snapshotOperations, IKeyOperations keyOperations,
			IDisciplineOperations disciplineOperations,IMediaOperations mediaOperations,
			IChatOperations messageOperations,
			INotificationOperations notificationOperations, IMiscellaneousOperations miscellaneousOperations)
		{
			env = environment;
			log = logger;

			accounts = accountOperations;
			connections = connectionOperations;
			nests = nestOperations;
			gatherings = gatheringOperations;
			snapshots = snapshotOperations;
			keys = keyOperations;
			reports = disciplineOperations;
			media = mediaOperations;
			messages = messageOperations;
			notifications = notificationOperations;
			miscellaneous = miscellaneousOperations;
		}
	}
}
