using FastEndpoints;
using LazyLizardBackend.Contracts.Responses;
using Mappers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Circle
{
    public class GetUserCircles(ICircleService circles) : EndpointWithoutRequest<List<CircleDTO>, CircleResponseMapper>
    {
        public override void Configure()
        {
            Get("/circle");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<CoreCircle> coreCircles = await circles.GetUserCirclesAsync(userId);
            await Send.OkAsync(coreCircles.Select(Map.FromEntity).ToList(), cancellationToken);
        }
    }
}