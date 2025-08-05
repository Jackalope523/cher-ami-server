using Microsoft.Extensions.Logging;
using Repository.Contexts;
using Repository.Repositories;

namespace Repository
{
    public class Harbor()
    {
        public enum Flag { Development, Staging, Production }


        public IAccountRepository AccountDatabaseAccess { get; private set; }
        public ICircleRepository CircleDatabaseAccess { get; private set; }
        public IIssueRepository IssueDatabaseAccess { get; private set; }
        public IKeyRepository KeyDatabaseAccess { get; private set; }
        public IMediaRepository MediaDatabaseAccess { get; private set; }
        public IMiscellaneousRepository MiscellaneousDatabaseAccess { get; private set; }
        public INotificationRepository NotificationDatabaseAccess { get; private set; }
        public IOrderRepository OrderDatabaseAccess { get; private set; }
        public IProfileRepository ProfileDatabaseAccess { get; private set; }
        public IReportRepository ReportDatabaseAccess { get; private set; }

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
            ProfileDatabaseAccess = new ProfileRepository(factory);
            NotificationDatabaseAccess = new NotificationRepository(factory);
            CircleDatabaseAccess = new CircleRepository(factory);
            IssueDatabaseAccess = new IssueRepository(factory);
            ReportDatabaseAccess = new ReportRepository(factory);
            KeyDatabaseAccess = new KeyStoreRepository();
            MediaDatabaseAccess = new MediaRepository(factory, storageAccountUri);
            MiscellaneousDatabaseAccess = new MiscellaneousRepository(factory);
            OrderDatabaseAccess = new OrderRepository();
        }
    }
}
