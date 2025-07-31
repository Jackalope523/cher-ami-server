using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class DeleteAccount(IAccountOperations accounts) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Delete("/account");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await accounts.DeleteUserAsync(userId);
            await Send.NoContentAsync(cancellationToken);
        }
    }
}
