using CherAmiAPI.Interfaces;
using CrazyLizard.Contexts;
using CrazyLizard.Entities;
using CrazyLizard.Exceptions;
using CrazyLizard.Shared.Requests;
using FastEndpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Media
{
    public class GetRecipientAvatarEndpoint(ApplicationDbContext ctx, IImageService imageService) : Endpoint<IdRequest, FileStreamResult>
    {
        public override void Configure()
        {
            Get("/recipients/{id}/avatar");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var result = await ctx.Recipients.Where(x => x.Id == request.Id).Select(x => new { x.ManagerId, x.AvatarPath}).SingleAsync(cancellationToken: cancellationToken);

            if (userId != result.ManagerId)
            {
                int count = await ctx.Users.Where(x => x.Id == userId || x.Id == result.ManagerId).Select(x => x.CircleId).Distinct().CountAsync(cancellationToken: cancellationToken);

                if (count > 1)
                    throw new NoAccessException($"User {userId} can not access this avatar.");
            }

            MemoryStream image = await imageService.DownloadImageAsync(result.AvatarPath);
            await Send.StreamAsync(image, cancellation: cancellationToken);
        }
    }
}