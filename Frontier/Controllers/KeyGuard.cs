using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Frontier.Manifests;
using Core.Boundaries;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Frontier.Controllers
{
	[Route("keys")]
	public class KeyGuard : AbstractGuard
	{
		#region Initialisation

		public KeyGuard(GuardBox box, UserManager<CoreUser> aspUserManager) : base(box, aspUserManager)
        { }

		#endregion

		#region Actions

		[HttpGet("map")]
		public async Task<IActionResult> GetMapKey()
		{
			return await Execute(async user =>
				await keys.GetCanaryMapKeyAsync(user.Id));
        }

		#endregion
	}
}