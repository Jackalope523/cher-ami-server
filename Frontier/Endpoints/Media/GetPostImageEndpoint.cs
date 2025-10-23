using CrazyLizard.Contexts;
using CrazyLizard.Entities;
using CrazyLizard.Exceptions;
using CrazyLizard.Interfaces.Repository;
using CrazyLizard.Interfaces.Service;
using CrazyLizard.Repositories;
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
    public class GetPostImageEndpoint(ApplicationDbContext ctx, IImageService imageService) : Endpoint<IdRequest, FileStreamResult>
    {
        public override void Configure()
        {
            Get("/posts/{id}/image");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            long? userCircle = await ctx.Users.Where(x => x.Id == userId).Select(x => x.CircleId).SingleOrDefaultAsync(cancellationToken: cancellationToken);
            long postCircle = await ctx.Posts.Where(x => x.Id == request.Id).Select(x => x.Issue.CircleId).SingleAsync(cancellationToken: cancellationToken);

            if (userCircle != postCircle)
                throw new NoAccessException($"User {userId} does not have access to post {request.Id}.");

            string path = await ctx.Posts.Where(x => x.Id == request.Id).Select(x => x.ImagePath).SingleAsync(cancellationToken: cancellationToken);
            MemoryStream image = await imageService.DownloadImageAsync(path);
            await Send.StreamAsync(image, cancellation: cancellationToken);
        }
    }
}