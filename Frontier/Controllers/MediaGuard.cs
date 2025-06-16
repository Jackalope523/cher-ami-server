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
	[Route("media")]
	public class MediaGuard : AbstractGuard
	{
		#region Initialisation

		public MediaGuard(GuardBox box, UserManager<CoreUser> aspUserManager) : base(box, aspUserManager)
		{ }

        #endregion

        #region Actions

        [HttpGet("assets/{asset}")]
		public async Task<IActionResult> GetAsset(string asset)
        {
            return await ExecuteUnsafe(async () =>
            {
                var user = await GetCurrentUserAsync();

                ThrowIfUnverified(user);

                var imageStream = await media.GetAssetAsync(asset);

                if (imageStream != null)
                {
                    imageStream.Seek(0, SeekOrigin.Begin);

                    return new FileStreamResult(imageStream, "image/jpeg")
                    {
                        FileDownloadName = $"{asset}.png"
                    };
                }

                throw new UnexpectedFailureException("Could not download image.");
            });
        }

		[HttpGet("avatars/{userId}")]
		public async Task<IActionResult> GetAvatar(long userId)
        {
            return await ExecuteUnsafe(async () =>
            {
                var user = await GetCurrentUserAsync();

                ThrowIfUnverified(user);

                var imageStream = await media.GetAvatarAsync(user.Id, userId);

                if (imageStream != null)
                {
                    imageStream.Seek(0, SeekOrigin.Begin);

                    return new FileStreamResult(imageStream, "image/jpeg")
                    {
                        FileDownloadName = "avatar.jpg"
                    };
                }

                throw new UnexpectedFailureException("Could not download image.");
            });
        }

		[HttpGet("avatars/{userId}/metadata")]
		public async Task<IActionResult> GetAvatarMetadata(long userId)
        {
            return await Execute(async user => await media.GetAvatarMetadataAsync(user.Id, userId));
        }

		[HttpGet("headers/{gatheringId}")]
		public async Task<IActionResult> GetHeader(long gatheringId)
        {
			return await ExecuteUnsafe(async () =>
			{
				var user = await GetCurrentUserAsync();

				ThrowIfUnverified(user);

				var imageStream = await media.GetHeaderAsync(user.Id, gatheringId);

				if (imageStream != null)
				{
					imageStream.Seek(0, SeekOrigin.Begin);

					return new FileStreamResult(imageStream, "image/jpeg")
					{
						FileDownloadName = "header.jpg"
					};
				}
				
				throw new UnexpectedFailureException("Could not download image.");
			});
        }

        [HttpGet("headers/{gatheringId}/metadata")]
        public async Task<IActionResult> GetHeaderMetadata(long gatheringId)
        {
            return await Execute(async user => await media.GetHeaderMetadataAsync(user.Id, gatheringId));
        }

        [HttpGet("snapshots/{snapshotId}")]
		public async Task<IActionResult> GetSnapshotImage(long snapshotId)
        {
            return await ExecuteUnsafe(async () =>
            {
                var user = await GetCurrentUserAsync();

                ThrowIfUnverified(user);

                var imageStream = await media.GetSnapshotAsync(user.Id, snapshotId);

                if (imageStream != null)
                {
                    imageStream.Seek(0, SeekOrigin.Begin);

                    return new FileStreamResult(imageStream, "image/jpeg")
                    {
                        FileDownloadName = "snapshot.jpg"
                    };
                }

                throw new UnexpectedFailureException("Could not download image.");
            });
        }

        [HttpGet("snapshots/{snapshotId}/metadata")]
        public async Task<IActionResult> GetSnapshotMetadata(long snapshotId)
        {
            return await Execute(async user => await media.GetSnapshotMetadataAsync(user.Id, snapshotId));
        }

        [HttpGet("conversation/{conversationId}")]
		public async Task<IActionResult> GetGroupChatHeader(long conversationId)
        {
            return await ExecuteUnsafe(async () =>
            {
                var user = await GetCurrentUserAsync();

                ThrowIfUnverified(user);

                var imageStream = await media.GetGroupChatHeaderAsync(user.Id, conversationId);

                if (imageStream != null)
                {
                    imageStream.Seek(0, SeekOrigin.Begin);

                    return new FileStreamResult(imageStream, "image/jpeg")
                    {
                        FileDownloadName = "photo.jpg"
                    };
                }

                throw new UnexpectedFailureException("Could not download image.");
            });
        }

        [HttpGet("conversation/{conversationId}/metadata")]
        public async Task<IActionResult> GetGroupChatHeaderMetadata(long conversationId)
        {
            return await Execute(async user => await media.GetGroupChatHeaderMetadataAsync(user.Id, conversationId));
        }

        [HttpGet("conversation/{conversationId}/photos/{photoId}")]
        public async Task<IActionResult> GetPhoto(long conversationId, Guid photoId)
        {
            return await ExecuteUnsafe(async () =>
            {
                var user = await GetCurrentUserAsync();

                ThrowIfUnverified(user);

                var imageStream = await media.GetPhotoAsync(user.Id, conversationId, photoId);

                if (imageStream != null)
                {
                    imageStream.Seek(0, SeekOrigin.Begin);

                    return new FileStreamResult(imageStream, "image/jpeg")
                    {
                        FileDownloadName = "photo.jpg"
                    };
                }

                throw new UnexpectedFailureException("Could not download image.");
            });
        }

        [HttpGet("conversation/{conversationId}/photos/{photoId}/metadata")]
        public async Task<IActionResult> GetPhotoMetadata(long conversationId, Guid photoId)
        {
            return await Execute(async user => await media.GetPhotoMetadataAsync(user.Id, conversationId, photoId));
        }

        #endregion
    }
}