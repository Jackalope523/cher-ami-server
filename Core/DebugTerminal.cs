using System;
using System.Collections.Generic;
using System.Threading;
using Core.Controls;
using Core.Boundaries;
using Microsoft.Extensions.Logging;

namespace Core
{
    public class DebugTerminal : CoreTerminal
	{
		#region Variables

		private static object initLock = new();

		public IDebugDatabase DebugDatabase { get; init; }

		public IDebugOperations DebugOperations
			=> DebugDirector;

		internal DebugDirector DebugDirector { get; private set; }

		#endregion

		#region Initialisation

		public static DebugTerminal CreateDebugTerminal(ILogger logger,
            IAccountDatabase accountDatabase, IAdminDatabase adminDatabase, IConnectionDatabase connectionDatabase,
            IGatheringDatabase gatheringDatabase, ISnapshotDatabase snapshotDatabase,
            IDisciplineDatabase disciplineDatabase, IKeyDatabase keyDatabase,
            IMediaDatabase mediaDatabase, IMessageDatabase messageDatabase,
			INotificationDatabase notificationDatabase, INestDatabase nestDatabase,
			IMiscellaneousDatabase miscellaneousDatabase,
            INotificationService notificationService, ISocketService socketService,
			IDebugDatabase debugDatabase)
		{
			lock (initLock)
			{
				Terminal ??= new DebugTerminal()
				{
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
					DebugDatabase = debugDatabase,
                };

                return (DebugTerminal) Terminal;
			}
		}

		protected DebugTerminal()
			: base()
		{
			DebugDirector = new DebugDirector(this);
		}

		#endregion
	}
}