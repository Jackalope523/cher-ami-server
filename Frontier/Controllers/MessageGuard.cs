using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Frontier.Manifests;
using Core.Boundaries;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net.Http.Headers;
using Repository;
using Twilio.TwiML.Voice;
using System.Security.Cryptography;

namespace Frontier.Controllers
{
	[Route("messages")]
	public class MessageGuard : AbstractGuard
	{
		#region Initialisation

		public MessageGuard(GuardBox box, UserManager<CoreUser> aspUserManager) : base(box, aspUserManager)
		{ }

        #endregion

        #region Actions

        [HttpGet]
        public async Task<IActionResult> GetConversations()
        {
            return await Execute(async user =>
            {
                return await messages.GetConversationsAsync(user.Id);
            });
        }

        [HttpGet("announcements")]
        public async Task<IActionResult> GetAnnouncements(string locale = "en")
        {
            return await Execute(async user =>
            {
                return await messages.GetAnnouncementsAsync(user.Id, locale);
            });
        }

        [HttpGet("user/{targetId}")]
        public async Task<IActionResult> GetConversationWith(long targetId)
        {
            return await Execute(async user =>
            {
                return await messages.GetConversationWithAsync(user.Id, targetId);
            });
        }

        [HttpPost("user/{targetId}")]
        public async Task<IActionResult> GetOrCreateConversationWith(long targetId)
        {
            return await Execute(async user =>
            {
                return await messages.GetOrCreateConversationWithAsync(user.Id, targetId);
            });
        }

        [HttpGet("gathering/{gatheringId}")]
        public async Task<IActionResult> GetGatheringConversation(long gatheringId)
        {
            return await Execute(async user =>
            {
                return await messages.GetGatheringConversationAsync(user.Id, gatheringId);
            });
        }

        [HttpPost("gathering/{gatheringId}")]
        public async Task<IActionResult> GetOrCreateGatheringConversation(long gatheringId)
        {
            return await Execute(async user =>
            {
                return await messages.GetOrCreateGatheringConversationAsync(user.Id, gatheringId);
            });
        }

        [HttpGet("{conversationId}")]
        public async Task<IActionResult> GetConversation(long conversationId)
        {
            return await Execute(async user =>
            {
                return await messages.GetConversationAsync(user.Id, conversationId);
            });
        }

        [HttpGet("{conversationId}/messages")]
		public async Task<IActionResult> GetConversationMessages(long conversationId, int page_number)
        {
            return await Execute(async user =>
            {
                return await messages.GetMessagesAsync(user.Id, conversationId, page_number);
            });
        }

        [HttpGet("{conversationId}/members")]
		public async Task<IActionResult> GetConversationMembers(long conversationId)
        {
            return await Execute(async user =>
            {
                return await messages.GetMembersAsync(user.Id, conversationId);
            });
        }

        [HttpPost]
		public async Task<IActionResult> CreateGroupChat([FromForm] GroupChatCreationManifest manifest)
        {
            if (manifest == null || !ModelState.IsValid)
            { return MissingInformation(); }

            return await Execute(async user =>
            {
                return await messages.CreateGroupChatAsync(user.Id, manifest.ParticipantIds);
            });
        }

        [HttpPost("{conversationId}")]
		public async Task<IActionResult> EditGroupChat(long conversationId, [FromForm] GroupChatEditManifest details)
        {
            return await Execute(async user =>
            {
                using var stream = new MemoryStream();
                if (details.Image != null && details.Image.Length > 0)
                {
                    await details.Image.CopyToAsync(stream);
                }

                await messages.EditGroupChatAsync(user.Id, conversationId,
                    title: details.Title,
                    header: stream);
            });
        }

        [HttpPut("{conversationId}")]
		public async Task<IActionResult> LeaveGroupChat(long conversationId)
        {
            return await Execute(async user =>
            {
                await messages.LeaveGroupChatAsync(user.Id, conversationId);
            });
        }

        [HttpDelete("{conversationId}")]
		public async Task<IActionResult> DeleteGroupChat(long conversationId)
        {
            return await Execute(async user =>
            {
                await messages.DeleteGroupChatAsync(user.Id, conversationId);
            });
        }

        [HttpPost("{conversationId}/members")]
        public async Task<IActionResult> SummonUser(long conversationId, long target_id)
        {
            return await Execute(async user =>
            {
                await messages.SummonUserAsync(user.Id, conversationId, target_id);
            });
        }

        [HttpPut("{conversationId}/members")]
        public async Task<IActionResult> KickUser(long conversationId, long target_id)
        {
            return await Execute(async user =>
            {
                await messages.KickUserAsync(user.Id, conversationId, target_id);
            });
        }

        [HttpPost("{conversationId}/photo")]
        public async Task<IActionResult> SendPhoto(long conversationId, [FromForm] ImageManifest photo)
        {
            // Verify parameters
            if (photo == null || !ModelState.IsValid ||
                photo.Image == null || photo.Image.Length == 0)
            { return MissingInformation(); }

            return await Execute(async user =>
            {
                using var stream = new MemoryStream();
                await photo.Image.CopyToAsync(stream);

                return await messages.SendPhotoAsync(user.Id, conversationId, stream);
            });
        }

        #endregion
    }
}