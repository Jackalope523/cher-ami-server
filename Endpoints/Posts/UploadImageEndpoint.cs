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

namespace CherAmiAPI.Endpoints.Posts
{
    public class UploadImageRequest
    {
        public long Id { get; set; }
        public IFormFile Image { get; set; }
    }

    public class UploadImageEndpoint(ApplicationDbContext ctx, IImageService imageService) : Endpoint<UploadImageRequest>
    {
        public override void Configure()
        {
            Post("/issue/posts/upload-image");
            AllowFileUploads();
        }

        public override async Task HandleAsync(UploadImageRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Post post = await ctx.Posts
                .IgnoreQueryFilters()
                .Include(x => x.Issue)
                .ThenInclude(x => x.Circle)
                .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (post == null || post.AuthorId != userId)
            {
                await Send.NotFoundAsync(cancellationToken);
                return;
            }

            using var stream = new MemoryStream();
            await request.Image.CopyToAsync(stream, cancellationToken);
            stream.Position = 0;

            string path = $"circles/{post.Issue.CircleId}/issues/{post.IssueId}/posts/{post.Id}/original.jpg";
            await imageService.UploadImageAsync(path, stream);

            post.HighResolutionImagePath = path;
            await ctx.SaveChangesAsync(cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}