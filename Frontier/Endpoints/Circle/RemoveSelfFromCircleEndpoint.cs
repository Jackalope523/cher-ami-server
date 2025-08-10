using FastEndpoints;
using Frontier.Contracts.Requests;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace LazyLizardBackend.Endpoints.Circle
{
    public class RemoveSelfFromCircleEndpoint(ICircleService circles) : Endpoint<CircleIdRequest>
    {
        public override void Configure()
        {
            Put("/circle/{circleId}/members");
        }

        public override async Task HandleAsync(CircleIdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await circles.RemoveMemberAsync(userId, request.Id);
            await Send.NoContentAsync(cancellationToken);
        }
    }
}
