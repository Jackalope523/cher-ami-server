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
            Func<CanaryContext> factory;

            switch (flag)
            {
                case Flag.Development:
                    factory = () => new DevelopmentContext();
                    break;
                case Flag.Staging:
                    factory = () => new AzureStagingContext();
                    break;
                case Flag.Production:
                    factory = () => new AzureProductionContext();
                    break;
                default:
                    throw new ArgumentException("Invalid Harbor flag: " + nameof(flag));
            }

            AccountDatabaseAccess = new EFCoreAccountStore(factory);
            ConnectionDatabaseAccess = new EFCoreConnectionStore(factory);
            ProfileDatabaseAccess = new EFCoreNestStore(factory);
            NotificationDatabaseAccess = new EFCoreNotificationStore(factory);
            CircleDatabaseAccess = new EFCoreGatheringStore(factory);
            IssueDatabaseAccess = new EFCoreSnapshotStore(factory);
            ReportDatabaseAccess = new EFCoreDisciplineStore(factory);
            KeyDatabaseAccess = new AzureKeyStore();
            MediaDatabaseAccess = new AzureFileStore(flag);
            ChatDatabaseAccess = new EFMessageStore(factory);
            DebugDatabaseAccess = new EFCoreDebugStore(factory);
            MiscellaneousDatabaseAccess = new EFCoreMiscellaneousStore(factory);
        }

        public Harbor(Flag flag, ILogger logger) : this(flag)
        {
            Harbor.logger = logger;
        }
    }
}
