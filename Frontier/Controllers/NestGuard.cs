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
    [Route("nest")]
    public class NestGuard : AbstractGuard
	{
		#region Initialisation

		public NestGuard(GuardBox box, UserManager<CoreUser> aspUserManager) : base(box, aspUserManager)
		{ }

		#endregion

		#region Actions

		[HttpGet("{targetId}")]
        public async Task<IActionResult> GetNest(long targetId)
		{
			return await Execute(async user =>
				await nests.GetNestAsync(user.Id, targetId));
		}

		[HttpGet("companions")]
        public async Task<IActionResult> GetCompanions()
        {
            return await Execute(async user =>
                await nests.GetCompanionsAsync(user.Id));
        }

        [HttpGet("companions/incoming")]
        public async Task<IActionResult> GetIncomingCompanionshipRequests()
		{
			return await Execute(async user =>
				await nests.GetIncomingCompanionshipRequestsAsync(user.Id));
		}

        [HttpGet("companions/outgoing")]
        public async Task<IActionResult> GetOutgoingCompanionshipRequests()
		{
			return await Execute(async user =>
				await nests.GetOutgoingCompanionshipRequestsAsync(user.Id));
		}

        [HttpGet("companions/recent")]
        public async Task<IActionResult> GetRecentlyMet()
		{
			return await Execute(async user =>
				await nests.GetRecentlyMetAsync(user.Id));
		}

		[HttpPost("companions")]
		public async Task<IActionResult> AcceptOrRequestUser(long? target_id = null, string code = null)
		{
			// Verify parameters
			if (!ModelState.IsValid)
			{ return MissingInformation(); }

			return await Execute(async user =>
			{
				if (target_id.HasValue)
				{ await nests.AcceptOrRequestCompanionshipAsync(user.Id, target_id.Value); }
				else if (!string.IsNullOrEmpty(code))
				{ await nests.RequestCompanionshipAsync(user.Id, code); }
				else
				{ throw new MissingInformationException(); }
			});
		}

		[HttpPut("companions")]
		public async Task<IActionResult> DenyOrRemoveUser(long target_id)
		{
			// Verify parameters
			if (!ModelState.IsValid)
			{ return MissingInformation(); }

			return await Execute(async user =>
				await nests.DenyOrRemoveUserAsync(user.Id, target_id));
        }

        [HttpPost("code")]
        public async Task<IActionResult> RerollUserCode()
        {
            return await Execute(async user =>
                await accounts.RerollCodeAsync(user.Id));
        }

        [HttpGet("blocked")]
		public async Task<IActionResult> GetBlocked()
		{
			return await Execute(async user =>
				await nests.GetBlockedUsersAsync(user.Id));
		}

		[HttpPost("blocked")]
		public async Task<IActionResult> BlockUser(long target_id)
		{
			// Verify parameters
			if (!ModelState.IsValid)
			{ return MissingInformation(); }

			return await Execute(async user =>
				await nests.BlockUserAsync(user.Id, target_id));
		}

		[HttpPut("blocked")]
		public async Task<IActionResult> UnblockUser(long target_id)
		{
			if (!ModelState.IsValid)
			{ return MissingInformation(); }

			return await Execute(async user =>
				await nests.UnblockUserAsync(user.Id, target_id));
		}

		[HttpGet("{targetId}/authorisation/follow")]
		public async Task<IActionResult> CheckFollowAuthorisation(long targetId)
		{
			return await Execute(async user =>
				await nests.AuthorisedToFollow(user.Id, targetId));
		}

		[HttpGet("{targetId}/report")]
		public async Task<IActionResult> AvailableUserReports(long targetId)
		{
			return await Execute(async user =>
				await reports.GetAvailableReportsForUserAsync(user.Id, targetId)
			);
		}

		[HttpPost("{targetId}/report")]
		public async Task<IActionResult> ReportUser(long targetId, [FromBody] AccountReportManifest report)
		{
			// Verify parameters
			if (report == null || !ModelState.IsValid)
			{ return MissingInformation(); }

			return await Execute(async user =>
				await reports.ReportUserAsync(user.Id, targetId, report.ReportType, report.ReportDetails, report.OccuringGatheringId)
			);
		}

		#endregion
	}
}