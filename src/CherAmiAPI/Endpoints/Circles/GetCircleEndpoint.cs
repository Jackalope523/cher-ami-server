using CherAmiAPI.Entities;
using CherAmiAPI.Services;
using CherAmiAPI.Shared.Responses;
using CherAmiAPI.Shared.SharedMappers;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Circles
{
    public class GetCircleEndpoint(CircleService circleService) : EndpointWithoutRequest<CircleDTO, CircleResponseMapper>
    {
        public override void Configure()
        {
            Get("/circle");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Circle circle = await circleService.GetCircleAsync(userId, cancellationToken);

            if (circle == null)
            {
                await Send.NoContentAsync(cancellationToken);
            }
            else
            {
                await Send.OkAsync(Map.FromEntity(circle), cancellationToken);
            }
        }
    }
}
