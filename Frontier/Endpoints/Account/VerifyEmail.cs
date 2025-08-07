using FastEndpoints;
using LazyLizardBackend.Contracts.Requests;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;
namespace Frontier.Endpoints.Account
{
    public class VerifyEmail(UserManager<CoreUser> userManager) : Endpoint<VerifyEmailRequest>
    {
        public override void Configure()
        {
            Get("/account/email");
            AllowAnonymous();
        }

        public override async Task HandleAsync(VerifyEmailRequest request, CancellationToken cancellationToken)
        {

            CoreUser user = await userManager.FindByEmailAsync(request.Email);

            if (user != null)
            {
                await userManager.ConfirmEmailAsync(user, request.Token);
                await Send.NoContentAsync(cancellationToken);
            }
            else
            {
                await Send.NotFoundAsync(cancellationToken);
            }
        }
    }
}