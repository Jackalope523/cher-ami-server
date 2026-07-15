using CherAmiAPI.Entities;
using CherAmiAPI.Exceptions;
using CherAmiAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Services
{
    public class PostService(
        IPostRepository postRepository,
        ICircleRepository circleRepository,
        IUserRepository userRepository,
        IImageService imageService,
        IUnitOfWork unitOfWork,
        ImageUploadCoordinator coordinator)
    {
        public async Task AddPostAsync(long userId, DateTime time, string caption, IFormFile image, int imageWidth, int imageHeight, CancellationToken cancellationToken = default)
        {
            long? circleId = await circleRepository.GetCircleIdOfUserAsync(userId, cancellationToken);
            long currentIssueId = await postRepository.GetCurrentIssueIdAsync(circleId.Value, cancellationToken);

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                Post postToAdd = new()
                {
                    IssueId = currentIssueId,
                    AuthorId = userId,
                    PostedAt = time,
                    Caption = caption,
                    ImageWidth = imageWidth,
                    ImageHeight = imageHeight,
                };

                await postRepository.AddPostAsync(postToAdd, cancellationToken);

                using var stream = new MemoryStream();
                await image.CopyToAsync(stream, cancellationToken);

                string path = $"circles/{circleId}/issues/{currentIssueId}/posts/{postToAdd.Id}/{Guid.NewGuid()}.jpg";

                postToAdd.LowResolutionImagePath = path;
                await postRepository.SavePostAsync(postToAdd, cancellationToken);

                await imageService.UploadImageAsync(path, stream);
            }, cancellationToken);
        }

        public async Task DeletePostAsync(long userId, long postId, CancellationToken cancellationToken = default)
        {
            Post post = await postRepository.GetPostAsync(postId, cancellationToken);

            if (post.AuthorId != userId)
                throw new NoAccessException($"User {userId} is the not the author of post {post.Id}.");

            await imageService.DeleteImageAsync(post.LowResolutionImagePath);

            await postRepository.RemovePostAsync(post, cancellationToken);
        }

        public async Task<int> GetLatestIssuePostCountAsync(long userId, CancellationToken cancellationToken = default)
        {
            long circleId = await circleRepository.GetCircleIdOfUserAsync(userId, cancellationToken)
                ?? throw new NotFoundException("User does not belong to a circle.");

            return await postRepository.GetLatestIssuePostCountAsync(circleId, cancellationToken);
        }

        public async Task ReportPostAsync(long userId, long postId, CancellationToken cancellationToken = default)
        {
            if (await postRepository.IsAuthorAsync(postId, userId, cancellationToken))
                throw new NoPermissionException("You can't report your own posts.");

            await postRepository.CreatePostReportAsync(postId, userId, cancellationToken);
        }

        public async Task<(Issue Issue, int? NextPage)> GetFeedPageAsync(long userId, int page, CancellationToken cancellationToken = default)
        {
            long circleId = await circleRepository.GetCircleIdOfUserAsync(userId, cancellationToken)
                ?? throw new NotFoundException("User does not have a circle.");

            List<long> blacklist = await userRepository.GetBlacklistedUserIdsAsync(userId, cancellationToken);

            Issue issue = await postRepository.GetFeedPageAsync(circleId, page, blacklist, cancellationToken);

            int? nextPage = null;
            if (issue != null)
            {
                int count = await postRepository.CountIssuesOfCircleAsync(circleId, cancellationToken);
                nextPage = count > page + 1 ? page + 1 : null;
            }

            return (issue, nextPage);
        }

        public async Task<MemoryStream> GetPostImageAsync(long userId, long postId, CancellationToken cancellationToken = default)
        {
            long? userCircle = await circleRepository.GetCircleIdOfUserAsync(userId, cancellationToken);
            long postCircle = await postRepository.GetCircleIdOfPostAsync(postId, cancellationToken);

            if (userCircle != postCircle)
                throw new NoAccessException($"User {userId} does not have access to post {postId}.");

            string path = await postRepository.GetLowResolutionImagePathAsync(postId, cancellationToken);

            return await imageService.DownloadImageAsync(path);
        }

        public async Task UploadImageAsync(long userId, string uploadId, IFormFile image, CancellationToken cancellationToken = default)
        {
            Log.Error("Starting image upload for User: {UserId}, UploadId: {UploadId}", userId, uploadId);

            Post post = await postRepository.GetPostByUploadIdAsync(uploadId, cancellationToken);

            if (post == null)
            {
                Log.Error("Post not found for UploadId: {UploadId}. Creating new post.", uploadId);
                long? circleId = await circleRepository.GetCircleIdOfUserAsync(userId, cancellationToken);

                if (circleId == null)
                {
                    Log.Error("User {UserId} has no CircleId.", userId);
                    throw new NoPermissionException($"User {userId} does not belong to a circle.");
                }

                long currentIssueId = await postRepository.GetCurrentIssueIdAsync(circleId.Value, cancellationToken);

                post = new()
                {
                    UploadId = uploadId,
                    AuthorId = userId,
                    IssueId = currentIssueId,
                    SoftDeleted = true
                };

                await postRepository.AddPostAsync(post, cancellationToken);

                // Reload to get include data
                post = await postRepository.GetPostByUploadIdAsync(uploadId, cancellationToken);

                Log.Error("Created new post with Id: {PostId} for UploadId: {UploadId}", post.Id, uploadId);
            }

            if (post.AuthorId != userId)
            {
                Log.Error("User {UserId} attempted to upload to post {PostId} owned by {AuthorId}", userId, post.Id, post.AuthorId);
                throw new NoPermissionException($"User {userId} is not the author of post {post.Id}.");
            }

            using var stream = new MemoryStream();
            await image.CopyToAsync(stream, cancellationToken);
            stream.Position = 0;

            string path = $"circles/{post.Issue.CircleId}/issues/{post.IssueId}/posts/{post.UploadId}/original.jpg";
            Log.Error("Uploading image to path: {Path}", path);
            await imageService.UploadImageAsync(path, stream);

            post.HighResolutionImagePath = path;
            await postRepository.SavePostAsync(post, cancellationToken);

            Log.Error("Image upload completed for UploadId: {UploadId}", uploadId);
            coordinator.MarkUploaded(uploadId);
        }

        public async Task<bool> ProcessImageDetailsAsync(long userId, string uploadId, string caption, int x, int y, int width, int height, CancellationToken cancellationToken = default)
        {
            await coordinator.WaitForUploadAsync(uploadId);

            Post post = await postRepository.GetPostByUploadIdAsync(uploadId, cancellationToken);

            if (post == null || post.AuthorId != userId)
                throw new NotFoundException($"Could not find a post for upload {uploadId}.");

            if (string.IsNullOrEmpty(post.HighResolutionImagePath))
                return false;

            // Download and process the image
            using var originalStream = await imageService.DownloadImageAsync(post.HighResolutionImagePath);
            using Image image = await Image.LoadAsync(originalStream, cancellationToken);

            // Crop the image
            image.Mutate(i => i.Crop(new Rectangle(x, y, width, height)));

            // Save the cropped high-res image
            using var croppedStream = new MemoryStream();
            await image.SaveAsJpegAsync(croppedStream, cancellationToken);
            croppedStream.Position = 0;

            string croppedPath = $"circles/{post.Issue.CircleId}/issues/{post.IssueId}/posts/{post.UploadId}/cropped.jpg";
            await imageService.UploadImageAsync(croppedPath, croppedStream);

            // Create and save the low-res version (resize to 800px width for example)
            image.Mutate(i => i.Resize(new ResizeOptions
            {
                Size = new Size(800, 0), // Aspect ratio maintained
                Mode = ResizeMode.Max
            }));

            using var lowResStream = new MemoryStream();
            await image.SaveAsJpegAsync(lowResStream, cancellationToken);
            lowResStream.Position = 0;

            string lowResPath = $"circles/{post.Issue.CircleId}/issues/{post.IssueId}/posts/{post.UploadId}/lowres.jpg";
            await imageService.UploadImageAsync(lowResPath, lowResStream);

            // Update database record
            post.Caption = caption;
            post.ImageWidth = width;
            post.ImageHeight = height;
            post.HighResolutionImagePath = croppedPath;
            post.LowResolutionImagePath = lowResPath;
            post.PostedAt = DateTimeOffset.UtcNow;
            post.SoftDeleted = false; // Finalize post

            await postRepository.SavePostAsync(post, cancellationToken);

            return true;
        }
    }
}
