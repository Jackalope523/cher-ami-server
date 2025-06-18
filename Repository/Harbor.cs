using Microsoft.Extensions.Logging;
using Repository.Coordinators;

namespace Repository
{
    public class Harbor
    {
        public enum Flag { Development, Staging, Production }

        internal static ILogger logger;

        public IAccountDatabase AccountDatabaseAccess { get; private set; }
        public IChatDatabase ChatDatabaseAccess { get; private set; }
        public IConnectionDatabase ConnectionDatabaseAccess { get; private set; }
        public ICircleDatabase CircleDatabaseAccess { get; private set; }
        public IIssueDatabase IssueDatabaseAccess { get; private set; }
        public IKeyDatabase KeyDatabaseAccess { get; private set; }
        public IMediaDatabase MediaDatabaseAccess { get; private set; }
        public IMiscellaneousDatabase MiscellaneousDatabaseAccess { get; private set; }
        public INotificationDatabase NotificationDatabaseAccess { get; private set; }
        public IProfileDatabase ProfileDatabaseAccess { get; private set; }
        public IReportDatabase ReportDatabaseAccess { get; private set; }

        public IDebugDatabase DebugDatabaseAccess { get; private set; }

        public Harbor(Flag flag)
        {
            AccountDatabaseAccess = new AccountStoreCoordinator(flag);
            ChatDatabaseAccess = new MessageStoreCoordinator(flag);
            ConnectionDatabaseAccess = new ConnectionStoreCoordinator(flag);
            CircleDatabaseAccess = new GatheringStoreCoordinator(flag);
            IssueDatabaseAccess = new SnapshotStoreCoordinator(flag);
            KeyDatabaseAccess = new KeyStoreCoordinator();
            MediaDatabaseAccess = new MediaStoreCoordinator(flag);
            MiscellaneousDatabaseAccess = new MiscellaneousStoreCoordinator(flag);
            NotificationDatabaseAccess = new NotificationStoreCoordinator(flag);
            ProfileDatabaseAccess = new NestStoreCoordinator(flag);
            ReportDatabaseAccess = new DisciplineStoreCoordinator(flag);

            DebugDatabaseAccess = new DebugStoreCoordinator(flag);
        }

        public Harbor(Flag flag, ILogger logger) : this(flag)
        {
            Harbor.logger = logger;
        }
    }
}
