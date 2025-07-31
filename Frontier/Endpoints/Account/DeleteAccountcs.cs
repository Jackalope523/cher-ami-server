using FastEndpoints;
using Frontier.Contracts.Requests;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class DeleteAccount(IAccountOperations accounts) : Endpoint<UserIdRequest, AccountShard>
    {
        public override void Configure()
        {
            Delete("/account");
        }

        public override async Task HandleAsync(UserIdRequest request, CancellationToken cancellationToken)
        {
            await accounts.DeleteUserAsync(request.UserId);
            await Send.NoContentAsync(cancellationToken); ;
        }
    }
}
