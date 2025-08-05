using Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Frontier.Controllers
{
	[ApiController]
	[Authorize]
	public class AbstractController : ControllerBase
	{
		#region Variables

		public EnvironmentOptions env;
		public ILogger log;

		public IAccountService accounts;
		public ICircleService circles;
		public IIssueService issues;
		public IKeyService keys;
		public IMediaService media;
		public IMiscellaneousService miscellaneous;
		public INotificationStorageService notifications;
		public IProfileService profiles;
		public IReportService reports;

		public UserManager<CoreUser> userManager;

		#endregion

		#region Initialisation

		public AbstractController(ControllerBox box, UserManager<CoreUser> aspUserManager)
		{
			env = box.env;
			log = box.log;

			accounts = box.accounts;
			circles = box.circles;
			issues = box.issues;
			keys = box.keys;
			media = box.media;
			miscellaneous = box.miscellaneous;
			notifications = box.notifications;
			profiles = box.profiles;
			reports = box.reports;

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

		// Execute and return an object to client
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

		// Execute and don't return anything to client
		[NonAction]
		public async Task<IActionResult> Execute(Func<Task> action)
		{
			return await Execute(async () =>
			{
				await action.Invoke();
				return "";
			});
		}

		// Execute as a user and don't return anything to client
		[NonAction]
		public async Task<IActionResult> Execute(Func<CoreUser, Task> action)
		{
			return await Execute(async user =>
			{
				await action.Invoke(user);
				return "";
			});
		}

		// Execute as a user and return an object to client
		[NonAction]
		public async Task<IActionResult> Execute(Func<CoreUser, Task<object>> action)
		{
			return await Execute(async () =>
			{
				var user = await GetCurrentUserAsync();

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
			ErrorShard error = new(System.Net.HttpStatusCode.BadRequest, "HOLLOW.MISSING_INFORMATION");
			return BadRequest(error);
		}

        #endregion
    }
}