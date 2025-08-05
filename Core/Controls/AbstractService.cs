using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Boundaries;
using Core.Entities;
using Microsoft.Extensions.Logging;
using static Core.Entities.Arbiter;

namespace Core.Controls
{
    public abstract class AbstractService
	{
		#region Variables

		protected CoreTerminal Terminal { get; init; }

		protected EnvironmentOptions Environment { get; init; }

		protected ILogger Log { get; private set; }

		protected IAccountRepository Accounts { get; private set; }
		protected ICircleRepository Circles { get; private set; }
		protected IIssueRepository Issues { get; private set; }
		protected IReportRepository Reports { get; private set; }
		protected IKeyRepository Keys { get; private set; }
		protected IMediaRepository Media { get; private set; }
		protected INotificationRepository Notifications { get; private set; }
		protected IOrderRepository Orders { get; private set; }
		protected IProfileRepository Profiles { get; private set; }
        protected IMiscellaneousRepository Miscellaneous { get; private set; }

        #endregion

        #region Initialisation

        public AbstractService(CoreTerminal terminal)
		{
			Terminal = terminal;
			Environment = terminal.Environment;

			Log = Terminal.Log;
			
			Accounts = Terminal.AccountDatabase;
			Circles = Terminal.CircleDatabase;
			Issues = Terminal.IssueDatabase;
			Reports = Terminal.ReportDatabase;
			Keys = Terminal.KeyDatabase;
			Media = Terminal.MediaDatabase;
			Notifications = Terminal.NotificationDatabase;
			Orders = Terminal.OrderDatabase;
			Profiles = Terminal.ProfileDatabase;
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

        protected async Task<Boundaries.CoreCircle> GetCircleAsync(long circleId)
        {
            return await Circles.GetCircleAsync(circleId);
        }

        #endregion
    }
}

