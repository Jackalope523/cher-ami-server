using CherAmiAPI.Interfaces;
using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Services;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Serilog;

namespace CherAmiAPI.Endpoints.Posts
{
    public class UploadImageDetailsRequest
    {
        public string UploadId { get; set; }
        public string Caption { get; set; }
        public DateTimeOffset? PhotoDate { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class UploadImageDetailsValidator : Validator<UploadImageDetailsRequest>
    {
        public UploadImageDetailsValidator()
        {
            RuleFor(x => x.Caption)
                .MaximumLength(200).WithMessage("Caption cannot exceed 200 characters.");
        }
    }

    public class UploadImageDetailsEndpoint(ApplicationDbContext ctx, IImageService imageService, ImageUploadCoordinator coordinator, IPhotoDateService photoDateService) : Endpoint<UploadImageDetailsRequest>
    {
        public override void Configure()
        {
            Post("/issue/posts/upload-details");
        }

        public override async Task HandleAsync(UploadImageDetailsRequest request, CancellationToken cancellationToken)
        {
            await coordinator.WaitForUploadAsync(request.UploadId);

            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var post = await ctx.Posts
                .IgnoreQueryFilters()
                .Where(x => x.UploadId == request.UploadId)
                .Select(x => new
                {
                    x.Id,
                    x.AuthorId,
                    x.IssueId,
                    x.HighResolutionImagePath,
                    x.Issue.CircleId,
                    x.Issue.DraftingStart,
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (post == null || post.AuthorId != userId)
            {
                await Send.NotFoundAsync(cancellationToken);
                return;
            }

            if (string.IsNullOrEmpty(post.HighResolutionImagePath))
            {
                //await Send.BadRequestAsync("High-resolution image not found. Upload it first.", cancellationToken);
                return;
            }

            // Download and process the image
            using var originalStream = await imageService.DownloadImageAsync(post.HighResolutionImagePath);
            using Image image = await Image.LoadAsync(originalStream, cancellationToken);

            // Crop the image
            image.Mutate(x => x.Crop(new Rectangle(request.X, request.Y, request.Width, request.Height)));

            // Save the cropped high-res image
            using var croppedStream = new MemoryStream();
            await image.SaveAsJpegAsync(croppedStream, cancellationToken);
            croppedStream.Position = 0;

            string croppedPath = $"circles/{post.CircleId}/issues/{post.IssueId}/posts/{request.UploadId}/cropped.jpg";
            await imageService.UploadImageAsync(croppedPath, croppedStream);

            // Create and save the low-res version (resize to 800px width for example)
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(800, 0), // Aspect ratio maintained
                Mode = ResizeMode.Max
            }));

            using var lowResStream = new MemoryStream();
            await image.SaveAsJpegAsync(lowResStream, cancellationToken);
            lowResStream.Position = 0;

            string lowResPath = $"circles/{post.CircleId}/issues/{post.IssueId}/posts/{request.UploadId}/lowres.jpg";
            await imageService.UploadImageAsync(lowResPath, lowResStream);

            // Update database record
            DateTimeOffset photoDate = photoDateService.Normalize(request.PhotoDate, post.DraftingStart);
            DateTimeOffset postedAt = DateTimeOffset.UtcNow;

            await ctx.Posts
                .IgnoreQueryFilters()
                .Where(x => x.Id == post.Id)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(p => p.Caption, request.Caption)
                    .SetProperty(p => p.ImageWidth, request.Width)
                    .SetProperty(p => p.ImageHeight, request.Height)
                    .SetProperty(p => p.HighResolutionImagePath, croppedPath)
                    .SetProperty(p => p.LowResolutionImagePath, lowResPath)
                    .SetProperty(p => p.PostedAt, postedAt)
                    .SetProperty(p => p.PhotoDate, photoDate)
                    .SetProperty(p => p.SoftDeleted, false), cancellationToken); // Finalize post

            await Send.NoContentAsync(cancellationToken);
        }
    }
}