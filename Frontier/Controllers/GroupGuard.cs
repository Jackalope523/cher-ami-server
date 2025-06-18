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
    [Route("group")]
    public class GroupGuard : AbstractGuard
	{
		#region Initialisation

		public GroupGuard(GuardBox box, UserManager<CoreUser> aspUserManager) : base(box, aspUserManager)
		{ }

		#endregion

		#region Actions

		[HttpGet]
        public async Task<IActionResult> GetUserGroups()
        {
			return await Execute(async user =>
			{
				return await groups.GetUserGroupsAsync(user.Id);
			});
        }

		[HttpGet("{groupId}")]
        public async Task<IActionResult> GetGroup(long groupId)
        {
			return await Execute(async user =>
			{
				return await groups.GetGroupInformationAsync(user.Id, groupId);
			});
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromForm] GroupCreationManifest groupDetails)
        {
			// Verify parameters
            if (groupDetails == null || !ModelState.IsValid ||
				groupDetails.Image == null || groupDetails.Image.Length == 0)
            { return MissingInformation(); }

			return await Execute(async user =>
            {
                using var stream = new MemoryStream();
                await groupDetails.Image.CopyToAsync(stream);

                return await groups.CreateGroupAsync(user.Id,
                    groupDetails.Title,
                    groupDetails.Plan,
                    groupDetails.Schedule,
                    stream);
            });
        }

        [HttpPost("{groupId}/edit")]
        public async Task<IActionResult> EditGroup(long groupId, [FromForm] GroupEditManifest groupDetails)
		{
			// Verify parameters
			if (groupDetails == null)
			{ return MissingInformation(); }

			return await Execute(async user =>
			{
                using var stream = new MemoryStream();
				if (groupDetails.Image != null && groupDetails.Image.Length > 0)
				{
					await groupDetails.Image.CopyToAsync(stream);
				}

				await groups.EditGroupAsync(user.Id, groupId,
					groupTitle: groupDetails.Title,
					plan: groupDetails.Plan,
					schedule: groupDetails.Schedule,
					header: stream);
			});
		}

        [HttpPost("{groupId}/code")]
        public async Task<IActionResult> RerollCode(long groupId)
		{
			return await Execute(async user =>
			{
				return await groups.RerollGroupCodeAsync(user.Id, groupId);
			});
        }

        [HttpDelete("{groupId}")]
        public async Task<IActionResult> DeleteGroup(long groupId)
		{
			return await Execute(async user =>
			{
				await groups.DeleteGroupAsync(user.Id, groupId);
			});
        }

		[HttpGet("{groupId}/members")]
		public async Task<IActionResult> GetMembers(long groupId)
		{
			return await Execute(async user =>
			{
				return await groups.GetMembersForGroupAsync(user.Id, groupId);
			});
		}

		[HttpPost("{groupId}/members")]
		public async Task<IActionResult> InviteUser(long groupId, long target_id)
		{
			return await Execute(async user =>
			{
				await groups.AddMemberAsync(user.Id, target_id, groupId);
			});
        }

		[HttpPut("{groupId}/members")]
		public async Task<IActionResult> RemoveUser(long groupId, long target_id)
		{
			return await Execute(async user =>
			{
				await groups.RemoveMemberAsync(user.Id, target_id, groupId);
			});
		}

		[HttpGet("{groupId}/recipients")]
		public async Task<IActionResult> GetRecipients(long groupId)
		{
			return await Execute(async user =>
			{
				return await groups.GetRecipientsForGroupAsync(user.Id, groupId);
			});
        }

		#endregion
	}
}