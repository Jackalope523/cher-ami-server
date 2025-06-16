using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Frontier.Manifests;
using Core.Boundaries;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.IO;

namespace Frontier.Controllers
{
    [Route("gathering")]
    public class GatheringGuard : AbstractGuard
	{
		#region Initialisation

		public GatheringGuard(GuardBox box, UserManager<CoreUser> aspUserManager) : base(box, aspUserManager)
		{ }

		#endregion

		#region Actions

		[HttpGet("{gatheringId}")]
        public async Task<IActionResult> GetGathering(long gatheringId)
        {
			return await Execute(async user =>
			{
				// Retrieve gathering information
				return await gatherings.GetGatheringInformationAsync(user.Id, gatheringId);
			});
        }

		[HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingGatherings()
        {
			return await Execute(async user =>
			{
				return await gatherings.GetUpcomingGatheringsAsync(user.Id);
			});
        }

		[HttpGet("ongoing")]
        public async Task<IActionResult> GetOngoingGatherings()
        {
			return await Execute(async user =>
			{
				return await gatherings.GetOngoingGatheringsAsync(user.Id);
			});
        }

		[HttpGet("past")]
        public async Task<IActionResult> GetPastGatherings()
        {
			return await Execute(async user =>
			{
				return await gatherings.GetPastGatheringsAsync(user.Id);
			});
        }

        [HttpPost]
        public async Task<IActionResult> CreateGathering([FromForm] GatheringCreationManifest gatheringDetails)
        {
			// Verify parameters
            if (gatheringDetails == null || !ModelState.IsValid ||
				gatheringDetails.Image == null || gatheringDetails.Image.Length == 0)
            { return MissingInformation(); }

			return await Execute(async user =>
            {
                using var stream = new MemoryStream();
                await gatheringDetails.Image.CopyToAsync(stream);

                // Create a new gathering
                return await gatherings.CreateGatheringAsync(user.Id,
                    gatheringDetails.Title, gatheringDetails.Description ?? "",
                    gatheringDetails.StartTime,
                    gatheringDetails.Latitude, gatheringDetails.Longitude, gatheringDetails.FriendlyLocation,
                    gatheringDetails.Radius, gatheringDetails.IsDynamic, gatheringDetails.DegreeOfPrivacy,
                    gatheringDetails.GroupMinimum, gatheringDetails.GroupMaximum,
                    stream);
            });
        }

        [HttpPost("{gatheringId}/edit")]
        public async Task<IActionResult> EditGathering(long gatheringId, [FromForm] GatheringEditManifest gatheringDetails)
		{
			// Verify parameters
			if (gatheringDetails == null)
			{ return MissingInformation(); }

			return await Execute(async user =>
			{
                using var stream = new MemoryStream();
				if (gatheringDetails.Image != null && gatheringDetails.Image.Length > 0)
				{
					await gatheringDetails.Image.CopyToAsync(stream);
				}

				await gatherings.EditGatheringAsync(user.Id, gatheringId,
					gatheringTitle: gatheringDetails.Title,
					gatheringDescription: gatheringDetails.Description,
					startTime: gatheringDetails.StartTime,
					latitude: gatheringDetails.Latitude, longitude: gatheringDetails.Longitude,
					friendlyLocation: gatheringDetails.FriendlyLocation,
					radius: gatheringDetails.Radius, isDynamic: gatheringDetails.IsDynamic,
					degreeOfPrivacy: gatheringDetails.DegreeOfPrivacy,
					groupMinimum: gatheringDetails.GroupMinimum, groupMaximum: gatheringDetails.GroupMaximum,
					header: stream);
			});
		}

        [HttpDelete("{gatheringId}/edit")]
        public async Task<IActionResult> EndGathering(long gatheringId)
		{
			return await Execute(async user =>
			{
				// End gathering
				await gatherings.TerminateGatheringAsync(user.Id, gatheringId);
			});
        }

        [HttpDelete("{gatheringId}")]
        public async Task<IActionResult> CancelGathering(long gatheringId)
		{
			return await Execute(async user =>
			{
				// Cancel gathering
				await gatherings.CancelGatheringAsync(user.Id, gatheringId);
			});
        }

        [HttpPost("{gatheringId}/visibility")]
        public async Task<IActionResult> HideGathering(long gatheringId, bool hide)
		{
			return await Execute(async user =>
			{
				await gatherings.ChangeGatheringVisibilityAsync(user.Id, gatheringId, hide);
			});
        }

		[HttpPost("{gatheringId}")]
        public async Task<IActionResult> JoinGathering(long gatheringId)
		{
			return await Execute(async user =>
			{
				// Join gathering
				await gatherings.JoinGatheringAsync(user.Id, gatheringId);
			});
		}

		[HttpPut("{gatheringId}")]
		public async Task<IActionResult> LeaveGathering(long gatheringId)
		{
			return await Execute(async user =>
			{
				// Leave gathering
				await gatherings.LeaveGatheringAsync(user.Id, gatheringId);
			});
		}

		[HttpGet("{gatheringId}/guests")]
		public async Task<IActionResult> GetGuestList(long gatheringId)
		{
			return await Execute(async user =>
			{
				return await gatherings.GetGuestListAsync(user.Id, gatheringId);
			});
		}

		[HttpGet("{gatheringId}/invite")]
		public async Task<IActionResult> GetPotentialInvitees(long gatheringId)
		{
			return await Execute(async user =>
			{
				return await gatherings.GetPotentialInviteesAsync(user.Id, gatheringId);
			});
        }

		[HttpPost("{gatheringId}/invite")]
		public async Task<IActionResult> InviteUser(long gatheringId, long target_id)
		{
			return await Execute(async user =>
			{
				await gatherings.InviteUserAsync(user.Id, target_id, gatheringId);
			});
        }

		[HttpPut("{gatheringId}/guests")]
		public async Task<IActionResult> KickUser(long gatheringId, long target_id)
		{
			return await Execute(async user =>
			{
				await gatherings.KickUserAsync(user.Id, target_id, gatheringId);
			});
		}

		[HttpGet("{gatheringId}/authorisation/join")]
		public async Task<IActionResult> CheckJoinAuthorisation(long gatheringId)
		{
			return await Execute(async user => await gatherings.AuthorisedToJoin(user.Id, gatheringId));
		}

		[HttpGet("{gatheringId}/authorisation/upload")]
		public async Task<IActionResult> CheckUploadAuthorisation(long gatheringId)
		{
			return await Execute(async user => await gatherings.AuthorisedToUpload(user.Id, gatheringId));
		}

		[HttpGet("{gatheringId}/report")]
		public async Task<IActionResult> AvailableGatheringReports(long gatheringId)
		{
			return await Execute(async user =>
				await reports.GetAvailableReportsForGatheringAsync(user.Id, gatheringId)
			);
		}

		[HttpPost("{gatheringId}/report")]
		public async Task<IActionResult> ReportGathering(long gatheringId, [FromBody] GatheringReportManifest report)
		{
			// Verify parameters
			if (report == null || !ModelState.IsValid)
			{ return MissingInformation(); }

			return await Execute(async user =>
			{
				await reports.ReportGatheringAsync(user.Id, gatheringId, report.ReportType, report.ReportDetails);
			});
		}

		[HttpGet("snapshot/{snapshotId}")]
        public async Task<IActionResult> GetSnapshot(long snapshotId)
        {
            return await Execute(async user =>
            {
                return await snapshots.GetSnapshotAsync(user.Id, snapshotId);
            });
        }

        [HttpGet("{gatheringId}/snapshots")]
		public async Task<IActionResult> GetGallery(long gatheringId)
		{
			return await Execute(async user =>
			{
				return await snapshots.GetGalleryAsync(user.Id, gatheringId);
			});
		}

		[HttpPost("{gatheringId}/snapshots")]
		public async Task<IActionResult> SnapshotGathering(long gatheringId, [FromForm] SnapshotManifest snapshot)
        {
            // Verify parameters
            if (snapshot == null || !ModelState.IsValid ||
                snapshot.Image == null || snapshot.Image.Length == 0)
            { return MissingInformation(); }

			return await Execute(async user =>
            {
                using var stream = new MemoryStream();
                await snapshot.Image.CopyToAsync(stream);

                return await snapshots.AddSnapshotAsync(user.Id, gatheringId, stream);
			});
		}

		[HttpPut("{gatheringId}/snapshots/{snapshotId}")]
		public async Task<IActionResult> RemoveSnapshot(long gatheringId, long snapshotId)
		{
			return await Execute(async user =>
			{
				await snapshots.DeleteSnapshotAsync(user.Id, snapshotId);
			});
		}

		[HttpPost("{gatheringId}/snapshots/{snapshotId}")]
		public async Task<IActionResult> AcclaimSnapshot(long gatheringId, long snapshotId, [FromBody] AccountRatingManifest details)
		{
			// Verify parameters
			if (details == null || !ModelState.IsValid)
			{ return MissingInformation(); }

			return await Execute(async user =>
			{
				await snapshots.AcclaimSnapshotAsync(user.Id, snapshotId, details.Action);
			});
		}

		[HttpGet("{gatheringId}/snapshots/{snapshotId}/report")]
		public async Task<IActionResult> AvailableSnapshotReports(long gatheringId, long snapshotId)
        {
            return await Execute(async user =>
				await reports.GetAvailableReportsForSnapshotAsync(user.Id, snapshotId)
            );
        }

        [HttpPost("{gatheringId}/snapshots/{snapshotId}/report")]
		public async Task<IActionResult> ReportSnapshot(long gatheringId, long snapshotId, [FromBody] SnapshotReportManifest report)
		{
			// Verify parameters
			if (report == null || !ModelState.IsValid)
			{ return MissingInformation(); }

			return await Execute(async user =>
            {
                await reports.ReportSnapshotAsync(user.Id, snapshotId, report.ReportType, report.ReportDetails);
            });
		}

		#endregion
	}
}