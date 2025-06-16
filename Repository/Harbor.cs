using Microsoft.Extensions.Logging;
using Repository.Coordinators;

namespace Repository
{
    public class Harbor
    {
        public enum Flag { Development, Staging, Production }

        internal static ILogger logger;

        public IAccountDatabase AccountDatabaseAccess { get; private set; }
        public IConnectionDatabase ConnectionDatabaseAccess { get; private set; }
        public INestDatabase NestDatabaseAccess { get; private set; }
        public INotificationDatabase NotificationDatabaseAccess { get; private set; }
        public IGatheringDatabase GatheringDatabaseAccess { get; private set; }
        public ISnapshotDatabase SnapshotDatabaseAccess { get; private set; }
        public IDisciplineDatabase ReportDatabaseAccess { get; private set; }
        public IAdminDatabase AdminDatabaseAccess { get; private set; }
        public IMediaDatabase MediaDatabaseAccess { get; private set; }
        public IMessageDatabase MessageDatabaseAccess { get; private set; }
        public IKeyDatabase KeyDatabaseAccess { get; private set; }
        public IDebugDatabase DebugDatabaseAccess { get; private set; }
        public IMiscellaneousDatabase MiscellaneousDatabaseAccess { get; private set; }

        public Harbor(Flag flag)
        {
            AccountDatabaseAccess = new AccountStoreCoordinator(flag);
            ConnectionDatabaseAccess = new ConnectionStoreCoordinator(flag);
            NestDatabaseAccess = new NestStoreCoordinator(flag);
            NotificationDatabaseAccess = new NotificationStoreCoordinator(flag);
            GatheringDatabaseAccess = new GatheringStoreCoordinator(flag);
            SnapshotDatabaseAccess = new SnapshotStoreCoordinator(flag);
            ReportDatabaseAccess = new DisciplineStoreCoordinator(flag);
            AdminDatabaseAccess = new AdminStoreCoordinator(flag);
            KeyDatabaseAccess = new KeyStoreCoordinator();
            MediaDatabaseAccess = new MediaStoreCoordinator(flag);
            MessageDatabaseAccess = new MessageStoreCoordinator(flag);
            DebugDatabaseAccess = new DebugStoreCoordinator(flag);
            MiscellaneousDatabaseAccess = new MiscellaneousStoreCoordinator(flag);
        }

        public Harbor(Flag flag, ILogger logger) : this(flag)
        {
            Harbor.logger = logger;
        }
    }
}
