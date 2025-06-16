using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Frontier.Manifests;
using Core.Boundaries;

using Microsoft.Extensions.Logging;

namespace Frontier.Controllers
{
    [Route("agenda")]
    public class AgendaGuard : AbstractGuard
	{
		#region Initialisation

		public AgendaGuard(GuardBox box, UserManager<CoreUser> aspUserManager) : base(box, aspUserManager)
		{ }

        #endregion

        #region Actions

        [HttpGet]
        public async Task<IActionResult> GetUserAgenda()
        {
            return await Execute(async user =>
                await nests.GetUserAgendaAsync(user.Id));
        }

        [HttpGet("companions")]
        public async Task<IActionResult> GetCompanionAgenda()
        {
            return await Execute(async user =>
                await nests.GetCompanionAgendasAsync(user.Id));
        }

        #endregion
    }
}