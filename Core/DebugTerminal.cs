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
            IAccountDatabase accountDatabase, IConnectionDatabase connectionDatabase,
            IGroupDatabase groupDatabase, IIssueDatabase issueDatabase,
            IReportDatabase reportDatabase, IKeyDatabase keyDatabase,
            IMediaDatabase mediaDatabase, IChatDatabase chatDatabase,
			INotificationDatabase notificationDatabase, IProfileDatabase profileDatabase,
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
					ChatDatabase = chatDatabase,
					ConnectionDatabase = connectionDatabase,
					GroupDatabase = groupDatabase,
					IssueDatabase = issueDatabase,
					KeyDatabase = keyDatabase,
					MediaDatabase = mediaDatabase,
					MiscellaneousDatabase = miscellaneousDatabase,
					NotificationDatabase = notificationDatabase,
					ProfileDatabase = profileDatabase,
					ReportDatabase = reportDatabase,

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