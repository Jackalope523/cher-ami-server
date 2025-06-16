using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Core.Boundaries;
using Microsoft.Extensions.Logging;
using Frontier.Manifests;

namespace Frontier.Controllers
{
    [Route("discover")]
    public class DiscoverGuard : AbstractGuard
	{
		#region Initialisation

		public DiscoverGuard(GuardBox box, UserManager<CoreUser> aspUserManager) : base(box, aspUserManager)
    { }

		#endregion

		#region Actions

		[HttpGet("{latitude},{longitude},{distance}")]
        public async Task<IActionResult> GetGatherings(float latitude, float longitude, float distance)
        {
			return await Execute(async user =>
			{
				// Retrieve gatherings personalised for the current user
				return await gatherings.GetPersonalisedGatheringsInAreaAsync(user.Id, latitude, longitude, distance);
			});
        }

        [HttpGet("all/{latitude},{longitude},{distance}")]
        public async Task<IActionResult> GetAllGatherings(float latitude, float longitude, float distance)
        {
			return await Execute(async user =>
			{
				// Retrieve all gatherings available to the current user
				return await gatherings.GetGatheringsInAreaAsync(user.Id, latitude, longitude, distance);
			});
        }

        [HttpPost("user/{latitude},{longitude}")]
        public async Task<IActionResult> UpdateCurrentPosition(float latitude, float longitude)
        {
			return await Execute(async user =>
			{
				await accounts.UpdateUserLocationAsync(user.Id, latitude, longitude);
			}, allowUnverified: true);
        }

		#endregion
	}
}