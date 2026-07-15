using CherAmiAPI.Services;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Circles
{
    public class LeaveCircleEndpoint(CircleService circleService) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Post("/circle/leave");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await circleService.LeaveCircleAsync(userId, cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
