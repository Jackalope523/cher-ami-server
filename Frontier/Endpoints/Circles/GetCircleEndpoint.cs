using FastEndpoints;
using CrazyLizard.Shared.SharedMappers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Shared.Responses;
using CrazyLizard.Interfaces.Service;
using CrazyLizard.Entities;
using Stripe;
using CrazyLizard.Contexts;
using Microsoft.EntityFrameworkCore;
using CrazyLizard.Exceptions;

namespace CrazyLizard.Endpoints.Circles
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
                throw new NotFoundException($"User {userId} does not have a circle.");

            Circle circle = await ctx.Circles.Where(x => x.Id == circleId).Include(x => x.Contributors).ThenInclude(x => x.Recipients).SingleAsync(cancellationToken: cancellationToken);
            
            await Send.OkAsync(Map.FromEntity(circle), cancellationToken);
        }
    }
}
