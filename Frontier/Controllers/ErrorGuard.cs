using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Core.Boundaries;
using Microsoft.Extensions.Logging;

namespace Frontier.Controllers
{
    [Route("error")]
    public class ErrorGuard : AbstractGuard
    {
		#region Initialisation

		public ErrorGuard(GuardBox box, UserManager<CoreUser> aspUserManager) : base(box, aspUserManager)
		{ }

		#endregion

		#region Actions

		[HttpGet]
        public IActionResult Error()
		{
			return new StatusCodeResult(418);
		}

		#endregion
	}
}