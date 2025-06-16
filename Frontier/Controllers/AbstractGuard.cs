using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Core.Boundaries;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Frontier.Manifests;
using System.Collections.Generic;
using System.Text;
using Core;

namespace Frontier.Controllers
{
	[ApiController]
	[Authorize]
	public class AbstractGuard : ControllerBase
	{
		#region Variables

		public EnvironmentOptions env;
		public ILogger log;

		public IAccountOperations accounts;
		public IConnectionOperations connections;
		public IGatheringOperations gatherings;
		public ISnapshotOperations snapshots;
		public IDisciplineOperations reports;
		public IKeyOperations keys;
		public IMediaOperations media;
		public IMessageOperations messages;
		public INestOperations nests;
		public INotificationOperations notifications;
		public IMiscellaneousOperations miscellaneous;

		public UserManager<CoreUser> userManager;

		#endregion

		#region Initialisation

		public AbstractGuard(GuardBox box, UserManager<CoreUser> aspUserManager)
		{
			env = box.env;
			log = box.log;

			accounts = box.accounts;
			connections = box.connections;
			nests = box.nests;
			gatherings = box.gatherings;
			snapshots = box.snapshots;
			keys = box.keys;
			reports = box.reports;
			media = box.media;
			messages = box.messages;
			notifications = box.notifications;
			miscellaneous = box.miscellaneous;

			userManager = aspUserManager;
		}

        #endregion

        #region Favours


        [NonAction]
        public async Task<IActionResult> ExecuteUnsafe(Func<Task<IActionResult>> action)
        {
            try
            {
                var result = await action.Invoke();

                // Check if there is a result
                if (result == null)
                {
                    Ok();
                }

                return result;
            }
            catch (UserErrorException ex)
            {
                // Log debug information
                log.LogDebug("\nUser Exception\n{message}\n{trace}", ex.Message, ex.StackTrace);

                return BadRequest(ex.ToErrorShard());
            }
			catch (HollowException ex)
            {
                // Get full exception message
                var message = DrillExceptionDetails(ex);

                // Log failure
                log.LogError("\nHollow Exception\n{message}\n{trace}", message, ex.StackTrace);

                return StatusCode(500, ex.ToErrorShard());
            }
            catch (Exception ex)
            {
				// Get full exception message
                var message = DrillExceptionDetails(ex);

                // Log failure
                log.LogError("\nHollow Exception\n{message}\n{trace}", message, ex.StackTrace);


                return StatusCode(500, HollowException.Default.ToErrorShard());
            }
        }

        [NonAction]
		public async Task<IActionResult> Execute(Func<Task<object>> action)
		{
			return await ExecuteUnsafe(async () =>
			{
				var result = await action.Invoke();

                // Ensure outgoing type is generic or manifest
                if (result is CoreOnlyData)
                { throw new UnexpectedFailureException($"Server tried sending Core-Only object {result.GetType()}.", code: HollowErrorCode.UNKNOWN); }

                return Ok(result);
			});
		}

		[NonAction]
		public async Task<IActionResult> Execute(Func<Task> action)
		{
			return await Execute(async () =>
			{
				await action.Invoke();
				return "";
			});
		}

		[NonAction]
		public async Task<IActionResult> Execute(Func<CoreUser, Task> action, bool allowUnverified = false)
		{
			return await Execute(async user =>
			{
				await action.Invoke(user);
				return "";
			},
			allowUnverified);
		}

		[NonAction]
		public async Task<IActionResult> Execute(Func<CoreUser, Task<object>> action, bool allowUnverified = false)
		{
			return await Execute(async () =>
			{
				var user = await GetCurrentUserAsync();

				if (!allowUnverified)
				{ ThrowIfUnverified(user); }

				return await action.Invoke(user);
			});
		}

		[NonAction]
		public async Task<CoreUser> GetCurrentUserAsync()
			=> await userManager.GetUserAsync(HttpContext.User);

		[NonAction]
		public void ThrowIfUnverified(CoreUser user)
		{
			if (!user.IsPhoneConfirmed)
			{ throw new UserErrorException(AccountErrorCode.UNVERIFIED); }
		}

		[NonAction]
		public string DrillExceptionDetails(Exception ex)
		{
			StringBuilder builder = new();

			while (ex != null)
			{
				builder.Append($"{ex.Message}, ");

				ex = ex.InnerException;
			}

			return builder.ToString();
		}


		[NonAction]
		public BadRequestObjectResult MissingInformation()
		{
			ErrorShard error = new("HOLLOW.MISSING_INFORMATION");
			return BadRequest(error);
		}

		#endregion
	}
}