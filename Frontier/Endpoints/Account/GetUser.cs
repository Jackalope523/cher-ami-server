using FastEndpoints;
using Frontier.Contracts.Requests;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class GetUser(IAccountOperations accounts) : Endpoint<UserIdRequest, UserShard>
    {
        public override void Configure()
        {
            Get("/account/{userId}");
        }

        public override async Task HandleAsync(UserIdRequest request, CancellationToken cancellationToken)
        {
            UserShard userShard = await accounts.GetUserShardAsync(request.UserId);

            if (userShard == null)
                await Send.NotFoundAsync(cancellationToken);

            await Send.OkAsync(userShard, cancellationToken);
        }
    }
}