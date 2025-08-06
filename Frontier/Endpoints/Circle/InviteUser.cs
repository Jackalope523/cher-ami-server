using FastEndpoints;
using Frontier.Contracts.Requests;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace LazyLizardBackend.Endpoints.Circle
{
    public class InviteUser(ICircleService circles) : Endpoint<CircleInviteRequest>
    {
        public override void Configure()
        {
            Post("/circle/{circleId}/members");
        }

        public override async Task HandleAsync(CircleInviteRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await circles.SendInvitationAsync(userId, request.CircleId, request.Phone_Number, request.Email);
            await Send.NoContentAsync(cancellationToken);
        }
    }
}