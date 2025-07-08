using System;
using Core.Boundaries;
using Core.Controls;

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

        public IAccountDatabase AccountDatabase { get; init; }
        public IChatDatabase ChatDatabase { get; init; }
        public IConnectionDatabase ConnectionDatabase { get; init; }
        public ICircleDatabase CircleDatabase { get; init; }
        public IIssueDatabase IssueDatabase { get; init; }
        public IKeyDatabase KeyDatabase { get; init; }
        public IMediaDatabase MediaDatabase { get; init; }
        public IMiscellaneousDatabase MiscellaneousDatabase { get; init; }
        public INotificationDatabase NotificationDatabase { get; init; }
        public IProfileDatabase ProfileDatabase { get; init; }
        public IReportDatabase ReportDatabase { get; init; }

        public IAccountOperations AccountOperations
            => AccountDirector;
        public IChatOperations ChatOperations
            => ChatDirector;
        public IConnectionOperations ConnectionOperations
            => ConnectionDirector;
        public ICircleOperations CircleOperations
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
        public IProfileOperations ProfileOperations
            => ProfileDirector;
        public IReportOperations ReportOperations
            => ReportDirector;

        public INotificationService NotificationService { get; init; }
        public ISocketService SocketService { get; init; }

        internal AccountDirector AccountDirector { get; private set; }
        internal ConnectionDirector ConnectionDirector { get; private set; }
        internal ChatDirector ChatDirector { get; private set; }
        internal CircleDirector CircleDirector { get; private set; }
        internal IssueDirector IssueDirector { get; private set; }
        internal KeyDirector KeyDirector { get; private set; }
        internal MediaDirector MediaDirector { get; private set; }
        internal MiscellaneousDirector MiscellaneousDirector { get; private set; }
        internal NotificationDirector NotificationDirector { get; private set; }
        internal ProfileDirector ProfileDirector { get; private set; }
        internal ReportDirector ReportDirector { get; private set; }

        #endregion

        #region Initialisation

        public static CoreTerminal CreateTerminal(EnvironmentOptions environment, ILogger logger,
            IAccountDatabase accountDatabase, IConnectionDatabase connectionDatabase,
            ICircleDatabase circleDatabase, IIssueDatabase issueDatabase,
            IReportDatabase reportDatabase, IKeyDatabase keyDatabase,
            IMediaDatabase mediaDatabase, IChatDatabase chatDatabase,
            INotificationDatabase notificationDatabase, IProfileDatabase profileDatabase,
            IMiscellaneousDatabase miscellaneousDatabase,
            INotificationService notificationService, ISocketService socketService)
        {
            lock (initLock)
            {
                Terminal ??= new CoreTerminal()
                {
                    Environment = environment,
                    Log = logger,

                    AccountDatabase = accountDatabase,
                    ChatDatabase = chatDatabase,
                    ConnectionDatabase = connectionDatabase,
                    CircleDatabase = circleDatabase,
                    IssueDatabase = issueDatabase,
                    KeyDatabase = keyDatabase,
                    MediaDatabase = mediaDatabase,
                    MiscellaneousDatabase = miscellaneousDatabase,
                    NotificationDatabase = notificationDatabase,
                    ProfileDatabase = profileDatabase,
                    ReportDatabase = reportDatabase,

                    NotificationService = notificationService,
                    SocketService = socketService,
                };

                Terminal.CreateManagers();

                return Terminal;
            }
        }

        protected CoreTerminal()
        { }

        protected void CreateManagers()
        {
            AccountDirector = new AccountDirector(this);
            ChatDirector = new ChatDirector(this);
            ConnectionDirector = new ConnectionDirector(this);
            CircleDirector = new CircleDirector(this);
            IssueDirector = new IssueDirector(this);
            KeyDirector = new KeyDirector(this);
            MediaDirector = new MediaDirector(this);
            MiscellaneousDirector = new MiscellaneousDirector(this);
            NotificationDirector = new NotificationDirector(this);
            ProfileDirector = new ProfileDirector(this);
            ReportDirector = new ReportDirector(this);
        }

        #endregion

        #region Daemons


        #endregion
    }
}