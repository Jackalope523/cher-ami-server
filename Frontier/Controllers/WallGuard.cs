using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Frontier.Manifests;
using Core.Boundaries;
using Microsoft.Extensions.Logging;

namespace Frontier.Controllers
{
    [Route("wall")]
    public class WallGuard : AbstractGuard
    {
		#region Initialisation

		public WallGuard(GuardBox box, UserManager<CoreUser> aspUserManager) : base(box, aspUserManager)
		{ }

		#endregion

		#region Actions

		[HttpGet]
        public async Task<IActionResult> GetWall(int depth, int last_depth)
        {
			// Verify parameters
            if (!ModelState.IsValid)
            { return MissingInformation(); }

			return await Execute(async user =>
				await snapshots.GetWallAsync(user.Id, depth, last_depth));
        }

		#endregion
	}
}