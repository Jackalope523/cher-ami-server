using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class Logout(SignInManager<CoreUser> userManager) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/account/logout");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            if (userManager.IsSignedIn(HttpContext.User))
            {
                await userManager.SignOutAsync();
            }

            await Send.NoContentAsync();
        }
    }
}