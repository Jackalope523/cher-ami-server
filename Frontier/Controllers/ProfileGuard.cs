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
    [Route("profile")]
    public class ProfileGuard : AbstractGuard
	{
		#region Initialisation

		public ProfileGuard(GuardBox box, UserManager<CoreUser> aspUserManager) : base(box, aspUserManager)
		{ }

		#endregion

		#region Actions

		[HttpGet("{targetId}")]
        public async Task<IActionResult> GetProfile(long targetId)
		{
			return await Execute(async user =>
				await profiles.GetProfileAsync(user.Id, targetId));
		}

        [HttpGet("blocked")]
		public async Task<IActionResult> GetBlocked()
		{
			return await Execute(async user =>
				await profiles.GetBlockedUsersAsync(user.Id));
		}

		[HttpPost("blocked")]
		public async Task<IActionResult> BlockUser(long target_id)
		{
			// Verify parameters
			if (!ModelState.IsValid)
			{ return MissingInformation(); }

			return await Execute(async user =>
				await profiles.BlockUserAsync(user.Id, target_id));
		}

		[HttpPut("blocked")]
		public async Task<IActionResult> UnblockUser(long target_id)
		{
			if (!ModelState.IsValid)
			{ return MissingInformation(); }

			return await Execute(async user =>
				await profiles.UnblockUserAsync(user.Id, target_id));
		}

		[HttpGet("{targetId}/report")]
		public async Task<IActionResult> AvailableUserReports(long targetId)
		{
			return await Execute(async user =>
				await reports.GetAvailableReportsForUserAsync(user.Id, targetId)
			);
		}

		[HttpPost("{targetId}/report")]
		public async Task<IActionResult> ReportUser(long targetId, [FromBody] UserReportManifest report)
		{
			// Verify parameters
			if (report == null || !ModelState.IsValid)
			{ return MissingInformation(); }

			return await Execute(async user =>
				await reports.ReportUserAsync(user.Id, targetId, report.ReportType, report.ReportDetails, report.GroupId)
			);
		}

		#endregion
	}
}