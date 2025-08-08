using FastEndpoints;
using Frontier.Contracts.Requests;
using Frontier.Contracts.Responses;
using Mappers;
using System.Threading;
using System.Threading.Tasks;

namespace LazyLizardBackend.Endpoints.Account
{
    public class GetUserEndpoint(IAccountService accountService) : Endpoint<UserIdRequest, UserDTO, UserResponseMapper>
    {
        public override void Configure()
        {
            Get("/account/{userId}");
        }

        public override async Task HandleAsync(UserIdRequest request, CancellationToken cancellationToken)
        {
            CoreUser userShard = await accountService.GetCoreUserAsync(request.UserId);

            if (userShard == null)
                await Send.NotFoundAsync(cancellationToken);

            await SendMappedAsync(userShard, 200, cancellationToken);
        }
    }
}