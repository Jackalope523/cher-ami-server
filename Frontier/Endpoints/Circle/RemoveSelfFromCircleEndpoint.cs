using Core.Boundaries;
using FastEndpoints;
using CrazyLizard.Contracts.Requests;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Circle
{
    public class RemoveSelfFromCircleEndpoint(ICircleService circles) : Endpoint<IdRequest>
    {
        public override void Configure()
        {
            Put("/circle/{circleId}/members");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await circles.RemoveMemberAsync(userId, request.Id);
            await Send.NoContentAsync(cancellationToken);
        }
    }
}
