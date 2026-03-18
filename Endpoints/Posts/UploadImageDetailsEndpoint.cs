using CherAmiAPI.Interfaces;
using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Shared.Mappers;
using CherAmiAPI.Shared.Requests;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace CherAmiAPI.Endpoints.Posts
{
    public class UploadImageDetailsRequest
    {
        public long Id { get; set; }
        public string Caption { get; set; }
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

    public class UploadImageDetailsEndpoint(ApplicationDbContext ctx, IImageService imageService) : Endpoint<UploadImageDetailsRequest>
    {
        public override void Configure()
        {
            Post("/issue/posts/upload-details");
        }

        public override async Task HandleAsync(UploadImageDetailsRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Post post = await ctx.Posts
                .IgnoreQueryFilters()
                .Include(x => x.Issue)
                .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (post == null || post.AuthorId != userId)
            {
                await Send.NotFoundAsync(cancellationToken);
                return;
            }

            if (string.IsNullOrEmpty(post.HighResolutionImagePath))
            {
                await Send.BadRequestAsync("High-resolution image not found. Upload it first.", cancellationToken);
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

            string croppedPath = $"circles/{post.Issue.CircleId}/issues/{post.IssueId}/posts/{post.Id}/cropped.jpg";
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

            string lowResPath = $"circles/{post.Issue.CircleId}/issues/{post.IssueId}/posts/{post.Id}/lowres.jpg";
            await imageService.UploadImageAsync(lowResPath, lowResStream);

            // Update database record
            post.Caption = request.Caption;
            post.ImageWidth = request.Width;
            post.ImageHeight = request.Height;
            post.HighResolutionImagePath = croppedPath;
            post.LowResolutionImagePath = lowResPath;
            post.SoftDeleted = false; // Finalize post

            await ctx.SaveChangesAsync(cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}