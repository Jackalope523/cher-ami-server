using CherAmiAPI.Exceptions;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Contexts;
using CherAmiAPI.Shared.Requests;
using FastEndpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Circles
{
    public class GetHeaderEndpoint(ApplicationDbContext ctx, IImageService imageService) : Endpoint<IdRequest, FileStreamResult>
    {
        public override void Configure()
        {
            Get("/circle/{id}/header");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));


            if (!await ctx.Users.AnyAsync(u => u.Id == userId && u.CircleId == request.Id))
                throw new NoAccessException($"User {userId} can not access this avatar.");

            string path = await ctx.Circles.Where(x => x.Id == request.Id).Select(x => x.HeaderPath).SingleAsync(cancellationToken: cancellationToken);
            MemoryStream image = await imageService.DownloadImageAsync(path);
            await Send.StreamAsync(image, cancellation: cancellationToken);
        }
    }
}