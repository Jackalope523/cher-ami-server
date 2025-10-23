using CherAmiAPI.Interfaces;
using CrazyLizard.Contexts;
using CrazyLizard.Exceptions;
using CrazyLizard.Shared.Requests;
using FastEndpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Users
{
    public class GetUserAvatarEndpoint(ApplicationDbContext ctx, IImageService imageService) : Endpoint<IdRequest, FileStreamResult>
    {
        public override void Configure()
        {
            Get("/users/{id}/avatar");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (userId != request.Id)
            {
                int count = await ctx.Users.Where(x => x.Id == userId || x.Id == request.Id).Select(x => x.CircleId).Distinct().CountAsync(cancellationToken: cancellationToken);

                if (count > 1)
                    throw new NoAccessException($"User {userId} can not access this avatar.");
            }

            string path = await ctx.Users.Where(x => x.Id == request.Id).Select(x => x.AvatarPath).SingleAsync(cancellationToken: cancellationToken);
            MemoryStream image = await imageService.DownloadImageAsync(path);
            await Send.StreamAsync(image, cancellation: cancellationToken);
        }
    }
}