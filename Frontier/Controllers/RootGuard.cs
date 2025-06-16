using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Core.Boundaries;
using Microsoft.Extensions.Logging;
using Frontier.Manifests;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace Frontier.Controllers
{
    [Route("")]
	[AllowAnonymous]
    public class RootGuard : AbstractGuard
    {
		#region Initialisation

		public RootGuard(GuardBox box, UserManager<CoreUser> aspUserManager) : base(box, aspUserManager)
		{ }

		#endregion

		#region Actions

		[HttpGet]
        public IActionResult IAmRoot()
        {
            return new StatusCodeResult(418);
        }

		[HttpGet("req")]
		public IActionResult CanaryMinimumVersion()
		{
			return Ok(new CanaryDetailsManifest() {
				MinimumVersion = "3.0.0",
				ServerVersion = "RoyalFrost",
				PageSize = 10,
			});
		}

		[HttpPost("feedback")]
		public async Task<IActionResult> Feedback([FromBody] FeedbackManifest feedback)
        {
            // Verify parameters
            if (feedback == null || !ModelState.IsValid)
            { return MissingInformation(); }

            return await Execute(user =>
			{
				if (string.IsNullOrEmpty(feedback.Pseudonym))
				{
					return miscellaneous.ReceiveFeedback(user.Id, feedback.Comments);
				}
				else
				{
					return miscellaneous.ReceiveAnonymousFeedback(user.Id, feedback.Pseudonym, feedback.Comments);
				}
			});
        }

		#endregion
	}
}