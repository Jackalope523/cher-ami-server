using FastEndpoints;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Users
{
    public class LogoutEndpoint() : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/account/logout");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            // Maybe invalidate refresh token when we have those.

            await Send.NoContentAsync(cancellationToken);
        }
    }
}