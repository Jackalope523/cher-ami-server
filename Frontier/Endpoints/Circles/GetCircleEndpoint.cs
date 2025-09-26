using FastEndpoints;
using CrazyLizard.Shared.SharedMappers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Shared.Responses;
using CrazyLizard.Entities;
using CrazyLizard.Contexts;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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

            Circle circle = await ctx.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Circle)
                .FirstOrDefaultAsync(cancellationToken);

            await Send.OkAsync(Map.FromEntity(circle), cancellationToken);
        }
    }
}