using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Frontier.Controllers
{
	[Route("media")]
	public class MediaController : AbstractController
	{
		#region Initialisation

		public MediaController(ControllerBox box, UserManager<CoreUser> aspUserManager) : base(box, aspUserManager)
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

                throw new UnexpectedFailureException($"Could not download image. asset:{asset}");
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

                throw new UnexpectedFailureException($"Could not download image. avatar:{userId}");
            });
        }

		[HttpGet("avatars/{userId}/metadata")]
		public async Task<IActionResult> GetAvatarMetadata(long userId)
        {
            return await Execute(async user => await media.GetAvatarMetadataAsync(user.Id, userId));
        }

		[HttpGet("headers/{circleId}")]
		public async Task<IActionResult> GetHeader(long circleId)
        {
			return await ExecuteUnsafe(async () =>
			{
				var user = await GetCurrentUserAsync();

				ThrowIfUnverified(user);

				var imageStream = await media.GetHeaderAsync(user.Id, circleId);

				if (imageStream != null)
				{
					imageStream.Seek(0, SeekOrigin.Begin);

					return new FileStreamResult(imageStream, "image/jpeg")
					{
						FileDownloadName = "header.jpg"
					};
				}
				
				throw new UnexpectedFailureException($"Could not download image. circle:{circleId}");
			});
        }

        [HttpGet("headers/{circleId}/metadata")]
        public async Task<IActionResult> GetHeaderMetadata(long circleId)
        {
            return await Execute(async user => await media.GetHeaderMetadataAsync(user.Id, circleId));
        }
      

        [HttpGet("posts/{postId}/metadata")]
        public async Task<IActionResult> GetPostMetadata(long postId)
        {
            return await Execute(async user => await media.GetPostMetadataAsync(user.Id, postId));
        }
        #endregion
    }
}