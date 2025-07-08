using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Boundaries;
using Core.Entities;
using Microsoft.Extensions.Logging;
using static Core.Entities.Arbiter;

namespace Core.Controls
{
    internal abstract class AbstractDirector
	{
		#region Variables

		protected CoreTerminal Terminal { get; init; }

		protected EnvironmentOptions Environment { get; init; }

		protected ILogger Log { get; private set; }

		protected IAccountDatabase Accounts { get; private set; }
		protected IConnectionDatabase Connections { get; private set; }
		protected ICircleDatabase Gatherings { get; private set; }
		protected IIssueDatabase Snapshots { get; private set; }
		protected IReportDatabase Reports { get; private set; }
		protected IKeyDatabase Keys { get; private set; }
		protected IMediaDatabase Media { get; private set; }
		protected IChatDatabase Messages { get; private set; }
		protected IProfileDatabase Nests { get; private set; }
		protected INotificationDatabase Notifications { get; private set; }
        protected IMiscellaneousDatabase Miscellaneous { get; private set; }

        #endregion

        #region Initialisation

        public AbstractDirector(CoreTerminal terminal)
		{
			Terminal = terminal;
			Environment = terminal.Environment;

			Log = Terminal.Log;
			
			Accounts = Terminal.AccountDatabase;
			Connections = Terminal.ConnectionDatabase;
			Gatherings = Terminal.CircleDatabase;
			Snapshots = Terminal.IssueDatabase;
			Reports = Terminal.ReportDatabase;
			Keys = Terminal.KeyDatabase;
			Media = Terminal.MediaDatabase;
			Messages = Terminal.ChatDatabase;
			Nests = Terminal.ProfileDatabase;
			Notifications = Terminal.NotificationDatabase;
			Miscellaneous = Terminal.MiscellaneousDatabase;
        }

		#endregion

		#region Tools

		protected async Task<User> GetUserAsync(long userId)
        {
            User user = new(await Accounts.GetUserByIdAsync(userId));
			
			// Fail if user account is locked
			FailIf(user.IsLocked,
				new UserErrorException(AccountErrorCode.LOCKED));

			// Fail if user account is pending deletion
			FailIf(user.IsDeleted,
				new UserErrorException(AccountErrorCode.DELETED));

            return user;
        }

		protected async Task<User> GetUserUnsafeAsync(long userId)
        {
            return new(await Accounts.GetUserByIdAsync(userId));
        }

        protected async Task<Issue> GetGatheringAsync(long gatheringId)
        {
            return new(await Gatherings.FindGatheringAsync(gatheringId));
        }

        protected async Task<Chat> GetConversationAsync(long chatId)
        {
            return new(await Messages.GetChatAsync(conversationId));
        }

        #endregion
    }
}

