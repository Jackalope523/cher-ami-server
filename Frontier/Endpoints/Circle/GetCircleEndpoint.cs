using FastEndpoints;
using CrazyLizard.Shared.SharedMappers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using CrazyLizard.Shared.Responses;
using CrazyLizard.Interfaces.Service;

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