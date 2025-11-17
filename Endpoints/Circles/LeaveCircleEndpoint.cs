using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Circles
{
    public class LeaveCircleEndpoint(ApplicationDbContext ctx) : EndpointWithoutRequest
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
            
            await Send.NoContentAsync(cancellationToken);
        }
    }
}
