using CrazyLizard.Interfaces.Service;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Users
{
    public class DeleteAccountEndpoint(IAccountService accounts) : EndpointWithoutRequest
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
