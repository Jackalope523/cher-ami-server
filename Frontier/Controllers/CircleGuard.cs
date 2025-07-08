using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Frontier.Manifests;
using System.IO;

namespace Frontier.Controllers
{
    [Route("circle")]
    public class CircleGuard : AbstractGuard
	{
		#region Initialisation

		public CircleGuard(GuardBox box, UserManager<CoreUser> aspUserManager) : base(box, aspUserManager)
		{ }

		#endregion

		#region Actions

		[HttpGet]
        public async Task<IActionResult> GetUserCircles()
        {
			return await Execute(async user =>
			{
				return await circles.GetUserCirclesAsync(user.Id);
			});
        }

		[HttpGet("{circleId}")]
        public async Task<IActionResult> GetCircle(long circleId)
        {
			return await Execute(async user =>
			{
				return await circles.GetCircleInformationAsync(user.Id, circleId);
			});
        }

        [HttpPost]
        public async Task<IActionResult> CreateCircle([FromForm] CircleCreationManifest circleDetails)
        {
			// Verify parameters
            if (circleDetails == null || !ModelState.IsValid ||
				circleDetails.Image == null || circleDetails.Image.Length == 0)
            { return MissingInformation(); }

			return await Execute(async user =>
            {
                using var stream = new MemoryStream();
                await circleDetails.Image.CopyToAsync(stream);

                return await circles.CreateCircleAsync(user.Id,
                    circleDetails.Title,
                    circleDetails.Plan,
                    circleDetails.Schedule,
                    stream);
            });
        }

        [HttpPost("{circleId}/edit")]
        public async Task<IActionResult> EditCircle(long circleId, [FromForm] CircleEditManifest circleDetails)
		{
			// Verify parameters
			if (circleDetails == null)
			{ return MissingInformation(); }

			return await Execute(async user =>
			{
                using var stream = new MemoryStream();
				if (circleDetails.Image != null && circleDetails.Image.Length > 0)
				{
					await circleDetails.Image.CopyToAsync(stream);
				}

				await circles.EditCircleAsync(user.Id, circleId,
					title: circleDetails.Title,
					plan: circleDetails.Plan,
					schedule: circleDetails.Schedule,
					header: stream);
			});
		}

        [HttpPost("{circleId}/code")]
        public async Task<IActionResult> RerollCode(long circleId)
		{
			return await Execute(async user =>
			{
				return await circles.RerollCircleCodeAsync(user.Id, circleId);
			});
        }

        [HttpDelete("{circleId}")]
        public async Task<IActionResult> DeleteCircle(long circleId)
		{
			return await Execute(async user =>
			{
				await circles.DeleteCircleAsync(user.Id, circleId);
			});
        }

		[HttpGet("{circleId}/members")]
		public async Task<IActionResult> GetMembers(long circleId)
		{
			return await Execute(async user =>
			{
				return await circles.GetMembersForCircleAsync(user.Id, circleId);
			});
		}

		[HttpPost("{circleId}/members")]
		public async Task<IActionResult> InviteUser(long circleId, long target_id)
		{
			return await Execute(async user =>
			{
				await circles.AddMemberAsync(user.Id, target_id, circleId);
			});
        }

		[HttpPut("{circleId}/members")]
		public async Task<IActionResult> RemoveUser(long circleId, long target_id)
		{
			return await Execute(async user =>
			{
				await circles.RemoveMemberAsync(user.Id, target_id, circleId);
			});
		}

		[HttpGet("{circleId}/recipients")]
		public async Task<IActionResult> GetRecipients(long circleId)
		{
			return await Execute(async user =>
			{
				return await circles.GetRecipientsForCircleAsync(user.Id, circleId);
			});
        }

		#endregion
	}
}