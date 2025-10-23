using CrazyLizard.Contexts;
using CrazyLizard.Endpoints.Issues;
using CrazyLizard.Entities;
using CrazyLizard.Interfaces.Service;
using CrazyLizard.Shared.Mappers;
using CrazyLizard.Shared.Requests;
using CrazyLizard.Shared.Responses;
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

namespace CrazyLizard.Endpoints.Circles
{
    public class EditCircleHeaderEndpoint(ApplicationDbContext ctx, IImageService imageService) : Endpoint<ImageRequest>
    {
        public override void Configure()
        {
            Post("/circle/header");
            AllowFileUploads();
        }

        public override async Task HandleAsync(ImageRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Circle circle = await ctx.Users.Where(x => x.Id == userId).Select(x => x.Circle).SingleAsync(cancellationToken: cancellationToken);

            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                using var stream = new MemoryStream();
                await request.Image.CopyToAsync(stream, cancellationToken);

                string path = $"circles/{circle.Id}/header.jpg";

                circle.HeaderPath = path;
                circle.HeaderTimestamp = DateTimeOffset.UtcNow;
                await ctx.SaveChangesAsync(cancellationToken);

                await imageService.UploadImageAsync(path, stream);

                await transaction.CommitAsync(cancellationToken);

                await Send.NoContentAsync(cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}