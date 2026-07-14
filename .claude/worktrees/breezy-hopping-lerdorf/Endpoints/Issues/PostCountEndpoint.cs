using CherAmiAPI.Contexts;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CherAmiAPI.Exceptions;
using Serilog;

namespace CherAmiAPI.Endpoints.Issues
{
    public class PostCountEndpoint(ApplicationDbContext ctx) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/issue/posts/count");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            long circleId = await ctx.Users
                            .Where(u => u.Id == userId)
                            .Select(u => u.CircleId)
                            .SingleAsync(cancellationToken) ?? throw new NotFoundException("User does not belong to a circle.");

            int count = await ctx.Issues
                        .Where(x => x.CircleId == circleId)
                        .OrderByDescending(x => x.DraftingEnd)
                        .Select(x => x.Posts.Count)
                        .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            await Send.OkAsync(count, cancellationToken);
        }
    }
}