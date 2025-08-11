using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class UpdateUserAgreementEndpoint(IAccountService accountService) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Post("/account/agreement");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await accountService.UpdateUserAgreementAsync(userId);
            await Send.NoContentAsync();
        }
    }
}