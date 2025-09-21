using CrazyLizard.Entities;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Account
{
    public class LogoutEndpoint(SignInManager<User> userManager) : EndpointWithoutRequest
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