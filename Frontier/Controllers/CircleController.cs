using Frontier.Contracts.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Frontier.Controllers
{
    [Route("circle")]
    public class CircleController(UserManager<CoreUser> aspUserManager, ICircleService circles) : ControllerBase
    {
		#region Initialisation

		#endregion

		#region Actions
        [HttpPost]
        public async Task<IActionResult> CreateCircle([FromForm] CircleCreationManifest circleDetails)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            using MemoryStream stream = new();
            await circleDetails.Image.CopyToAsync(stream);

            CoreCircle coreCircle = await circles.CreateCircleAsync(
										userId,
										circleDetails.Title,
										circleDetails.Plan,
										circleDetails.Schedule,
										stream
									);

			CircleShard response = new CircleShard(
                coreCircle.Id,
                coreCircle.InviteCode,
                coreCircle.Title,
                coreCircle.DateCreated,
                coreCircle.Plan,
                coreCircle.Schedule
            );


            return Ok(response);
        }

        [HttpPost("{circleId}/edit")]
        public async Task<IActionResult> EditCircle(long circleId, [FromForm] CircleEditManifest circleDetails)
		{
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            using var stream = new MemoryStream();
            if (circleDetails.Image != null && circleDetails.Image.Length > 0)
            {
                await circleDetails.Image.CopyToAsync(stream);
            }

            await circles.EditCircleAsync(userId, circleId,
                title: circleDetails.Title,
                plan: circleDetails.Plan,
                schedule: circleDetails.Schedule,
                header: stream);

            return NoContent();
        }

        [HttpPost("{circleId}/code")]
        public async Task<IActionResult> RerollCode(long circleId)
		{
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            return Ok(await circles.RerollCircleCodeAsync(userId, circleId));
        }

        [HttpDelete("{circleId}")]
        public async Task<IActionResult> DeleteCircle(long circleId)
		{
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await circles.DeleteCircleAsync(userId, circleId);

            return NoContent();
        }

		[HttpGet("{circleId}/members")]
		public async Task<IActionResult> GetMembers(long circleId)
		{
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Ok(await circles.GetCircleMembers(userId, circleId));
        }

		[HttpPost("{circleId}/members")]
		public async Task<IActionResult> InviteUser(long circleId, string phone_number, string email)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await circles.SendInvitationAsync(userId, circleId, phone_number, email);
            return NoContent();
        }

		[HttpPut("{circleId}/members")]
		public async Task<IActionResult> RemoveCircleMember(long circleId, long target_id)
		{
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await circles.RemoveMemberAsync(userId, circleId);
            return NoContent();
        }

		[HttpGet("{circleId}/recipients")]
		public async Task<IActionResult> GetRecipients(long circleId)
		{
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Ok(await circles.GetRecipientsForCircleAsync(userId, circleId));
        }

		#endregion
	}
}