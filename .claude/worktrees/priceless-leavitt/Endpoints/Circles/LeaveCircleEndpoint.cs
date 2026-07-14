using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Interfaces;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Circles
{
    public class LeaveCircleEndpoint(ApplicationDbContext ctx, IImageService imageService) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Post("/circle/leave");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            User user = await ctx.Users.Where(x => x.Id == userId).SingleAsync(cancellationToken: cancellationToken);

            user.CircleId = null;
            user.CircleJoinDate = null;
            await ctx.SaveChangesAsync(cancellationToken);

            List<string> recipientAvatars = await ctx.Recipients.Where(x => x.ManagerId == userId).Select(x => x.AvatarPath).ToListAsync(cancellationToken: cancellationToken);
            await imageService.DeleteImagesAsync(recipientAvatars);

            await ctx.Recipients.Where(x => x.ManagerId == user.Id).ExecuteDeleteAsync(cancellationToken);
            
            await Send.NoContentAsync(cancellationToken);
        }
    }
}
