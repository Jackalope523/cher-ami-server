using Core;

namespace Frontier.Controllers
{
	public class GuardBox
	{
		public EnvironmentOptions env;
		public ILogger log;

		public IAccountOperations accounts;
		public IConnectionOperations connections;
		public IGatheringOperations gatherings;
		public ISnapshotOperations snapshots;
		public IKeyOperations keys;
		public IDisciplineOperations reports;
		public IMediaOperations media;
		public IMessageOperations messages;
		public INestOperations nests;
		public INotificationOperations notifications;
        public IMiscellaneousOperations miscellaneous;

        public GuardBox(EnvironmentOptions environment, ILogger logger,
			IAccountOperations accountOperations, IConnectionOperations connectionOperations,
			INestOperations nestOperations, IGatheringOperations gatheringOperations,
			ISnapshotOperations snapshotOperations, IKeyOperations keyOperations,
			IDisciplineOperations disciplineOperations,IMediaOperations mediaOperations,
			IMessageOperations messageOperations,
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
