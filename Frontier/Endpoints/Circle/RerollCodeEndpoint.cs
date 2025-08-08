using FastEndpoints;
using Frontier.Contracts.Requests;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace LazyLizardBackend.Endpoints.Circle
{
    public class RerollCodeEndpoint(ICircleService circles) : Endpoint<CircleIdRequest>
    {
        public override void Configure()
        {
            Post("/circle/{circleId}/code");
        }

        public override async Task HandleAsync(CircleIdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            string response = await circles.RerollCircleCodeAsync(userId, request.CircleId);
            await Send.OkAsync(response, cancellationToken);
        }
    }
}