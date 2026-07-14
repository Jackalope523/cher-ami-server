using CherAmiAPI.Interfaces;
using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Services;
using CherAmiAPI.Shared.Mappers;
using CherAmiAPI.Shared.Requests;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Posts
{
    public class UploadImageRequest
    {
        public string UploadId { get; set; }
        public IFormFile Image { get; set; }
    }

    public class UploadImageEndpoint(ApplicationDbContext ctx, IImageService imageService, ImageUploadCoordinator coordinator) : Endpoint<UploadImageRequest>
    {
        public override void Configure()
        {
            Post("/issue/posts/upload-image");
            AllowFileUploads();
        }

        public override async Task HandleAsync(UploadImageRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Log.Error("Starting image upload for User: {UserId}, UploadId: {UploadId}", userId, request.UploadId);

            Post post = await ctx.Posts
                .IgnoreQueryFilters()
                .Include(x => x.Issue)
                .ThenInclude(x => x.Circle)
                .SingleOrDefaultAsync(x => x.UploadId == request.UploadId, cancellationToken);

            if (post == null)
            {
                Log.Error("Post not found for UploadId: {UploadId}. Creating new post.", request.UploadId);
                long? circleId = await ctx.Users
                    .Where(x => x.Id == userId)
                    .Select(x => x.CircleId)
                    .SingleAsync(cancellationToken);

                if (circleId == null)
                {
                    Log.Error("User {UserId} has no CircleId.", userId);
                    await Send.ForbiddenAsync(cancellationToken);
                    return;
                }

                long currentIssueId = await ctx.Issues
                    .Where(x => x.CircleId == circleId)
                    .OrderByDescending(x => x.DraftingEnd)
                    .Select(x => x.Id)
                    .FirstAsync(cancellationToken);

                post = new()
                {
                    UploadId = request.UploadId,
                    AuthorId = userId,
                    IssueId = currentIssueId,
                    SoftDeleted = true
                };

                ctx.Posts.Add(post);
                await ctx.SaveChangesAsync(cancellationToken);

                // Reload to get include data if needed
                post = await ctx.Posts
                    .IgnoreQueryFilters()
                    .Include(x => x.Issue)
                    .ThenInclude(x => x.Circle)
                    .SingleAsync(x => x.Id == post.Id, cancellationToken);

                Log.Error("Created new post with Id: {PostId} for UploadId: {UploadId}", post.Id, request.UploadId);
            }

            if (post.AuthorId != userId)
            {
                Log.Error("User {UserId} attempted to upload to post {PostId} owned by {AuthorId}", userId, post.Id, post.AuthorId);
                await Send.ForbiddenAsync(cancellationToken);
                return;
            }

            using var stream = new MemoryStream();
            await request.Image.CopyToAsync(stream, cancellationToken);
            stream.Position = 0;

            string path = $"circles/{post.Issue.CircleId}/issues/{post.IssueId}/posts/{post.UploadId}/original.jpg";
            Log.Error("Uploading image to path: {Path}", path);
            await imageService.UploadImageAsync(path, stream);

            post.HighResolutionImagePath = path;
            await ctx.SaveChangesAsync(cancellationToken);

            Log.Error("Image upload completed for UploadId: {UploadId}", request.UploadId);
            coordinator.MarkUploaded(request.UploadId);
            await Send.NoContentAsync(cancellationToken);
        }
    }
}