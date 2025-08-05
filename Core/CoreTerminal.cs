using System;
using Core.Boundaries;
using Core.Services;

namespace Core
{
    public enum EnvironmentFlag
    {
        Production, Staging, Development
    }

    public record EnvironmentOptions
    {
        public EnvironmentFlag Flag { get; init; }

        public bool IsProduction => Flag.Equals(EnvironmentFlag.Production);
        public bool IsStaging => Flag.Equals(EnvironmentFlag.Staging);
    }

    public class CoreTerminal
    {
        #region Variables

        public static CoreTerminal Terminal { get; protected set; }
        private static object initLock = new();

        public EnvironmentOptions Environment { get; init; }

        public ILogger Log { get; init; }

        public IAccountRepository AccountDatabase { get; init; }
        public ICircleRepository CircleDatabase { get; init; }
        public IIssueRepository IssueDatabase { get; init; }
        public IKeyRepository KeyDatabase { get; init; }
        public IMediaRepository MediaDatabase { get; init; }
        public IMiscellaneousRepository MiscellaneousDatabase { get; init; }
        public INotificationRepository NotificationDatabase { get; init; }
        public IOrderRepository OrderDatabase { get; init; }
        public IProfileRepository ProfileDatabase { get; init; }
        public IReportRepository ReportDatabase { get; init; }

        public IAccountService AccountOperations
            => AccountDirector;
        public ICircleService CircleOperations
            => CircleDirector;
        public IIssueOperations IssueOperations
            => IssueDirector;
        public IKeyOperations KeyOperations
            => KeyDirector;
        public IMediaOperations MediaOperations
            => MediaDirector;
        public IMiscellaneousOperations MiscellaneousOperations
            => MiscellaneousDirector;
        public INotificationOperations NotificationOperations
            => NotificationDirector;
        public IOrderOperations OrderOperations
            => OrderDirector;
        public IProfileOperations ProfileOperations
            => ProfileDirector;
        public IReportOperations ReportOperations
            => ReportDirector;

        public INotificationService NotificationService { get; init; }

        internal AccountService AccountDirector { get; private set; }
        internal CircleService CircleDirector { get; private set; }
        internal IssueService IssueDirector { get; private set; }
        internal KeyService KeyDirector { get; private set; }
        internal MediaService MediaDirector { get; private set; }
        internal MiscellaneousService MiscellaneousDirector { get; private set; }
        internal NotificationService NotificationDirector { get; private set; }
        internal OrderService OrderDirector { get; private set; }
        internal ProfileService ProfileDirector { get; private set; }
        internal ReportService ReportDirector { get; private set; }

        #endregion

        #region Initialisation

        public static CoreTerminal CreateTerminal(EnvironmentOptions environment, ILogger logger,
            IAccountRepository accountDatabase,
            ICircleRepository circleDatabase, IIssueRepository issueDatabase,
            IReportRepository reportDatabase, IKeyRepository keyDatabase,
            IMediaRepository mediaDatabase,
            INotificationRepository notificationDatabase,
            IOrderRepository orderDatabase,
            IProfileRepository profileDatabase,
            IMiscellaneousRepository miscellaneousDatabase,
            INotificationService notificationService)
        {
            lock (initLock)
            {
                Terminal ??= new CoreTerminal()
                {
                    Environment = environment,
                    Log = logger,

                    AccountDatabase = accountDatabase,
                    CircleDatabase = circleDatabase,
                    IssueDatabase = issueDatabase,
                    KeyDatabase = keyDatabase,
                    MediaDatabase = mediaDatabase,
                    MiscellaneousDatabase = miscellaneousDatabase,
                    NotificationDatabase = notificationDatabase,
                    OrderDatabase = orderDatabase,
                    ProfileDatabase = profileDatabase,
                    ReportDatabase = reportDatabase,

                    NotificationService = notificationService,
                };

                Terminal.CreateManagers();

                return Terminal;
            }
        }

        protected CoreTerminal()
        { }

        protected void CreateManagers()
        {
            AccountDirector = new AccountService(this);
            CircleDirector = new CircleService(this);
            IssueDirector = new IssueService(this);
            KeyDirector = new KeyService(this);
            MediaDirector = new MediaService(this);
            MiscellaneousDirector = new MiscellaneousService(this);
            NotificationDirector = new NotificationService(this);
            OrderDirector = new OrderService(this);
            ProfileDirector = new ProfileService(this);
            ReportDirector = new ReportService(this);
        }

        #endregion

        #region Daemons


        #endregion
    }
}