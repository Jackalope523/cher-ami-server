using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Exceptions;
using CherAmiAPI.Shared.Responses;
using CherAmiAPI.Shared.SharedMappers;
using FastEndpoints;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Circles
{
    public class GetCircleEndpoint(ApplicationDbContext ctx) : EndpointWithoutRequest<CircleDTO, CircleResponseMapper>
    {
        public override void Configure()
        {
            Get("/circle");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            long? circleId = await ctx.Users.Where(x => x.Id == userId).Select(x => x.CircleId).SingleAsync(cancellationToken: cancellationToken);

            if (circleId == null)
            {
                await Send.NoContentAsync(cancellationToken);
            }
            else
            {
                List<long> blockedIds = await ctx.Blocks
                                        .Where(x => x.BlockerId == userId)
                                        .Select(x => x.BlockedId)
                                        .ToListAsync(cancellationToken: cancellationToken);

                List<long> blockedByIds = await ctx.Blocks
                                          .Where(x => x.BlockedId == userId)
                                          .Select(x => x.BlockerId)
                                          .ToListAsync(cancellationToken: cancellationToken);

                List<long> blacklist = [.. blockedIds, .. blockedByIds];

                Circle circle = await ctx.Circles
                                .Where(x => x.Id == circleId)
                                .Include(x => x.Contributors.Where(x => !blacklist.Contains(x.Id)))
                                .ThenInclude(x => x.Recipients.Where(x => !blacklist.Contains(x.ManagerId)))
                                .SingleAsync(cancellationToken: cancellationToken);

                await Send.OkAsync(Map.FromEntity(circle), cancellationToken);
            }
        }
    }
}
