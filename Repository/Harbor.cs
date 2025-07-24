using Microsoft.Extensions.Logging;
using Repository.Databases.Contexts;
using Repository.Databases.Stores;

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
            Func<CardinalContext> factory;
            string storageAccountUri = "https://{0}.blob.core.windows.net";

            switch (flag)
            {
                case Flag.Development:
                    factory = () => new DevelopmentContext();
                    storageAccountUri = storageAccountUri + "/sparrowstorageaccount";
                    break;
                case Flag.Staging:
                    factory = () => new AzureStagingContext();
                    storageAccountUri = storageAccountUri + "/sparrowstorageaccount";
                    break;
                case Flag.Production:
                    factory = () => new AzureProductionContext();
                    storageAccountUri = storageAccountUri + "/canaryproduction";
                    break;
                default:
                    throw new ArgumentException("Invalid Harbor flag: " + nameof(flag));
            }

            AccountDatabaseAccess = new AccountRepository(factory);
            ConnectionDatabaseAccess = new ConnectionRepository(factory);
            ProfileDatabaseAccess = new ProfileRepository(factory);
            NotificationDatabaseAccess = new NotificationRepository(factory);
            CircleDatabaseAccess = new CircleRepository(factory);
            IssueDatabaseAccess = new IssueRepository(factory);
            ReportDatabaseAccess = new ReportRepository(factory);
            KeyDatabaseAccess = new AzureKeyStore();
            MediaDatabaseAccess = new MediaRepository(factory, storageAccountUri);
            ChatDatabaseAccess = new ChatRepository(factory);
            DebugDatabaseAccess = new DebugRepository(factory);
            MiscellaneousDatabaseAccess = new MiscellaneousRepository(factory);
        }

        public Harbor(Flag flag, ILogger logger) : this(flag)
        {
            Harbor.logger = logger;
        }
    }
}
