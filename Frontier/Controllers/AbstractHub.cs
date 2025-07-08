using System;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using Core;

namespace Frontier.Controllers
{
	[Authorize]
	public partial class SocketHub : Hub<IClientSocket>
	{
		#region Variables

		public EnvironmentOptions env;
		public ILogger log;

		public IAccountOperations accounts;
		public IChatOperations chats;
		public IConnectionOperations connections;
		public ICircleOperations circles;
		public IIssueOperations issues;
		public IKeyOperations keys;
		public IMediaOperations media;
		public IMiscellaneousOperations miscellaneous;
		public INotificationOperations notifications;
		public IProfileOperations profiles;
		public IReportOperations reports;

		public UserManager<CoreUser> userManager;

		#endregion

		#region Initialisation

		public SocketHub(GuardBox box, UserManager<CoreUser> aspUserManager)
		{
			env = box.env;
			log = box.log;

			accounts = box.accounts;
			chats = box.chat;
			connections = box.connections;
			circles = box.circles;
			issues = box.issues;
			keys = box.keys;
			media = box.media;
			miscellaneous = box.miscellaneous;
			notifications = box.notifications;
			reports = box.reports;
			profiles = box.profiles;

			userManager = aspUserManager;
		}

        #endregion

        #region Favours

        protected async Task<T> ExecuteUnsafe<T>(Func<Task<T>> action)
        {
            try
            {
                return await action.Invoke();
            }
            catch (UserErrorException ex)
            {
                // Log debug information
                log.LogDebug("\nUser Exception\n{message}\n{trace}", ex.Message, ex.StackTrace);

				string error = Newtonsoft.Json.JsonConvert.SerializeObject(ex.ToErrorShard());

                throw new HubException(error);
            }
			catch (HollowException ex)
            {
                // Get full exception message
                var message = DrillExceptionDetails(ex);

                // Log failure
                log.LogError("\nHollow Exception\n{message}\n{trace}", message, ex.StackTrace);

                string error = Newtonsoft.Json.JsonConvert.SerializeObject(ex.ToErrorShard());

                throw new HubException(error);
            }
            catch (Exception ex)
            {
				// Get full exception message
                var message = DrillExceptionDetails(ex);

                // Log failure
                log.LogError("\nHollow Exception\n{message}\n{trace}", message, ex.StackTrace);

                string error = Newtonsoft.Json.JsonConvert.SerializeObject(HollowException.Default.ToErrorShard());

                throw new HubException(error);
            }
        }

		protected async Task<T> Execute<T>(Func<Task<T>> action)
		{
			return await ExecuteUnsafe(async () =>
			{
				var result = await action.Invoke();

                // Ensure outgoing type is generic or manifest
                if (result is CoreOnlyData)
                { throw new UnexpectedFailureException($"Server tried sending Core-Only object {result.GetType()}.", code: HollowErrorCode.UNKNOWN); }

                return result;
			});
		}

		protected async Task Execute(Func<Task> action)
		{
			await Execute(async () =>
			{
				await action.Invoke();
			});
		}

		protected async Task Execute(Func<CoreUser, Task> action, bool allowUnverified = false)
		{
			await Execute(async user =>
			{
				await action.Invoke(user);
			},
			allowUnverified);
		}

		protected async Task<T> Execute<T>(Func<CoreUser, Task<T>> action, bool allowUnverified = false)
		{
			return await Execute(async () =>
			{
				var user = await GetCurrentUserAsync();

				if (!allowUnverified)
				{ ThrowIfUnverified(user); }

				return await action.Invoke(user);
			});
		}

		protected async Task<CoreUser> GetCurrentUserAsync()
			=> await userManager.GetUserAsync(Context.User);

		protected void ThrowIfUnverified(CoreUser user)
		{
			if (!user.IsPhoneConfirmed)
			{ throw new UserErrorException(AccountErrorCode.UNVERIFIED); }
		}

		protected string DrillExceptionDetails(Exception ex)
		{
			StringBuilder builder = new();

			while (ex != null)
			{
				builder.Append($"{ex.Message}, ");

				ex = ex.InnerException;
			}

			return builder.ToString();
		}

        #endregion

        #region Socket

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            var user = await GetCurrentUserAsync();
            var connectionId = Context.ConnectionId;

            await connections.UserConnectedAsync(user.Id, connectionId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);

            var user = await GetCurrentUserAsync();
            var connectionId = Context.ConnectionId;

            await connections.UserDisconnectedAsync(user.Id, connectionId);
        }

        #endregion
    }
}