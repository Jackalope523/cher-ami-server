using System;
using Core.Boundaries;
using Core.Controls;
using Core.Daemons;

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
        public IAdminDatabase AdminDatabase { get; init; }
        public IConnectionDatabase ConnectionDatabase { get; init; }
        public IGatheringDatabase GatheringDatabase { get; init; }
        public ISnapshotDatabase SnapshotDatabase { get; init; }
        public IDisciplineDatabase DisciplineDatabase { get; init; }
        public IKeyDatabase KeyDatabase { get; init; }
        public IMediaDatabase MediaDatabase { get; init; }
        public IMessageDatabase MessageDatabase { get; init; }
        public INestDatabase NestDatabase { get; init; }
        public INotificationDatabase NotificationDatabase { get; init; }
        public IMiscellaneousDatabase MiscellaneousDatabase { get; init; }

        public IAccountOperations AccountOperations
            => AccountDirector;
        public IConnectionOperations ConnectionOperations
            => ConnectionDirector;
        public IGatheringOperations GatheringOperations
            => GatheringDirector;
        public ISnapshotOperations SnapshotOperations
            => SnapshotDirector;
        public IDisciplineOperations DisciplineOperations
            => DisciplineDirector;
        public IKeyOperations KeyOperations
            => KeyDirector;
        public IMediaOperations MediaOperations
            => MediaDirector;
        public IMessageOperations MessageOperations
            => MessageDirector;
        public INestOperations NestOperations
            => NestDirector;
        public INotificationOperations NotificationOperations
            => NotificationDirector;
        public IMiscellaneousOperations MiscellaneousOperations
            => MiscellaneousDirector;

        public INotificationService NotificationService { get; init; }
        public ISocketService SocketService { get; init; }

        internal AccountDirector AccountDirector { get; private set; }
        internal ConnectionDirector ConnectionDirector { get; private set; }
        internal GatheringDirector GatheringDirector { get; private set; }
        internal SnapshotDirector SnapshotDirector { get; private set; }
        internal DisciplineDirector DisciplineDirector { get; private set; }
        internal KeyDirector KeyDirector { get; private set; }
        internal MediaDirector MediaDirector { get; private set; }
        internal MessageDirector MessageDirector { get; private set; }
        internal NestDirector NestDirector { get; private set; }
        internal NotificationDirector NotificationDirector { get; private set; }
        internal MiscellaneousDirector MiscellaneousDirector { get; private set; }

        #endregion

        #region Initialisation

        public static CoreTerminal CreateTerminal(EnvironmentOptions environment, ILogger logger,
            IAccountDatabase accountDatabase, IAdminDatabase adminDatabase, IConnectionDatabase connectionDatabase,
            IGatheringDatabase gatheringDatabase, ISnapshotDatabase snapshotDatabase,
            IDisciplineDatabase disciplineDatabase, IKeyDatabase keyDatabase,
            IMediaDatabase mediaDatabase, IMessageDatabase messageDatabase,
            INotificationDatabase notificationDatabase, INestDatabase nestDatabase,
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
                    AdminDatabase = adminDatabase,
                    ConnectionDatabase = connectionDatabase,
                    GatheringDatabase = gatheringDatabase,
                    SnapshotDatabase = snapshotDatabase,
                    DisciplineDatabase = disciplineDatabase,
                    KeyDatabase = keyDatabase,
                    MediaDatabase = mediaDatabase,
                    MessageDatabase = messageDatabase,
                    NestDatabase = nestDatabase,
                    NotificationDatabase = notificationDatabase,
                    MiscellaneousDatabase = miscellaneousDatabase,

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
            ConnectionDirector = new ConnectionDirector(this);
            GatheringDirector = new GatheringDirector(this);
            SnapshotDirector = new SnapshotDirector(this);
            DisciplineDirector = new DisciplineDirector(this);
            KeyDirector = new KeyDirector(this);
            MediaDirector = new MediaDirector(this);
            MessageDirector = new MessageDirector(this);
            NestDirector = new NestDirector(this);
            NotificationDirector = new NotificationDirector(this);
            MiscellaneousDirector = new MiscellaneousDirector(this);
        }

        #endregion

        #region Daemons

        public GatheringOverseerGoblin CreateRepositoryCleanupService()
         => new(this);

        #endregion
    }
}