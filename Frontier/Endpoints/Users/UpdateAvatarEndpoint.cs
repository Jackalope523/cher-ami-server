using CrazyLizard.Contexts;
using CrazyLizard.Endpoints.Issues;
using CrazyLizard.Entities;
using CrazyLizard.Interfaces.Service;
using CrazyLizard.Shared.Requests;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Users
{
    public class UpdateUserAvatarEndpoint(ApplicationDbContext ctx, IImageService imageService) : Endpoint<ImageRequest>
    {
        public override void Configure()
        {
            Post("/user/avatar");
            AllowFileUploads();
        }

        public override async Task HandleAsync(ImageRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            User user = await ctx.Users.Where(x => x.Id == userId).SingleAsync(cancellationToken: cancellationToken);

            await using var transaction = await ctx.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                using var stream = new MemoryStream();
                await request.Image.CopyToAsync(stream, cancellationToken);

                string path = $"users/{user.Id}/avatar.jpg";

                user.AvatarPath = path;
                user.AvatarTimestamp = DateTimeOffset.UtcNow;
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