using FastEndpoints;
using CrazyLizard.Shared.SharedMappers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Shared.Responses;
using CrazyLizard.Interfaces.Service;
using CrazyLizard.Entities;

namespace CrazyLizard.Endpoints.Circles
{
    public class GetCircleEndpoint(ICircleService circles) : EndpointWithoutRequest<CircleDTO, CircleResponseMapper>
    {
        public override void Configure()
        {
            Get("/circle");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Circle circle = await circles.GetCircleForUserAsync(userId);

            if (circle == null)
            {
                await Send.OkAsync(null, cancellationToken);
            }
            else
            {
                await Send.OkAsync(Map.FromEntity(circle), cancellationToken);
            }
        }
    }
}