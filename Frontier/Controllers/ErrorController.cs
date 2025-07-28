using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Core.Boundaries;
using Microsoft.Extensions.Logging;

namespace Frontier.Controllers
{
    [Route("error")]
    public class ErrorController : AbstractController
    {
		#region Initialisation

		public ErrorController(ControllerBox box, UserManager<CoreUser> aspUserManager) : base(box, aspUserManager)
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