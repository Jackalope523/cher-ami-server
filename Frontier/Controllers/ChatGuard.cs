using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Frontier.Manifests;
using System.IO;

namespace Frontier.Controllers
{
	[Route("chat")]
	public class MessageGuard : AbstractGuard
	{
		#region Initialisation

		public MessageGuard(GuardBox box, UserManager<CoreUser> aspUserManager) : base(box, aspUserManager)
		{ }

        #endregion

        #region Actions

        [HttpGet]
        public async Task<IActionResult> GetChats()
        {
            return await Execute(async user =>
            {
                return await chats.GetChatsAsync(user.Id);
            });
        }

        [HttpGet("announcements")]
        public async Task<IActionResult> GetAnnouncements(string locale = "en")
        {
            return await Execute(async user =>
            {
                return await chats.GetAnnouncementsAsync(user.Id, locale);
            });
        }

        [HttpGet("user/{targetId}")]
        public async Task<IActionResult> GetChatWith(long targetId)
        {
            return await Execute(async user =>
            {
                return await chats.GetChatWithAsync(user.Id, targetId);
            });
        }

        [HttpPost("user/{targetId}")]
        public async Task<IActionResult> GetOrCreateChatWith(long targetId)
        {
            return await Execute(async user =>
            {
                return await chats.GetOrCreateChatWithAsync(user.Id, targetId);
            });
        }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetGroupChat(long groupId)
        {
            return await Execute(async user =>
            {
                return await chats.GetGroupChatAsync(user.Id, groupId);
            });
        }

        [HttpGet("{chatId}")]
        public async Task<IActionResult> GetChat(long chatId)
        {
            return await Execute(async user =>
            {
                return await chats.GetChatAsync(user.Id, chatId);
            });
        }

        [HttpGet("{chatId}/messages")]
		public async Task<IActionResult> GetChatMessages(long chatId, int page_number)
        {
            return await Execute(async user =>
            {
                return await chats.GetMessagesAsync(user.Id, chatId, page_number);
            });
        }

        [HttpGet("{chatId}/members")]
		public async Task<IActionResult> GetChatMembers(long chatId)
        {
            return await Execute(async user =>
            {
                return await chats.GetMembersAsync(user.Id, chatId);
            });
        }

        [HttpPost("{chatId}/photo")]
        public async Task<IActionResult> SendPhoto(long chatId, [FromForm] ImageManifest photo)
        {
            // Verify parameters
            if (photo == null || !ModelState.IsValid ||
                photo.Image == null || photo.Image.Length == 0)
            { return MissingInformation(); }

            return await Execute(async user =>
            {
                using var stream = new MemoryStream();
                await photo.Image.CopyToAsync(stream);

                return await chats.SendPhotoAsync(user.Id, chatId, stream);
            });
        }

        #endregion
    }
}