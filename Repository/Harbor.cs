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
            NestDatabaseAccess = new EFCoreNestStore(factory);
            NotificationDatabaseAccess = new EFCoreNotificationStore(factory);
            GatheringDatabaseAccess = new EFCoreGatheringStore(factory);
            SnapshotDatabaseAccess = new EFCoreSnapshotStore(factory);
            ReportDatabaseAccess = new EFCoreDisciplineStore(factory);
            AdminDatabaseAccess = new EFCoreAdminStore(factory);
            KeyDatabaseAccess = new AzureKeyStore();
            MediaDatabaseAccess = new AzureFileStore(flag);
            MessageDatabaseAccess = new EFMessageStore(factory);
            DebugDatabaseAccess = new EFCoreDebugStore(factory);
            MiscellaneousDatabaseAccess = new EFCoreMiscellaneousStore(factory);
        }

        public Harbor(Flag flag, ILogger logger) : this(flag)
        {
            Harbor.logger = logger;
        }
    }
}
