using Core.Boundaries;
using FastEndpoints;
using CrazyLizard.Contracts.Responses;
using CrazyLizard.Shared.SharedMappers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace CrazyLizard.Endpoints.Circle
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

            CoreCircle coreCircle = await circles.GetCircleForUserAsync(userId);

            if (coreCircle == null)
            {
                await Send.OkAsync(null, cancellationToken);
            }
            else
            {
                await Send.OkAsync(Map.FromEntity(coreCircle), cancellationToken);
            }
        }
    }
}