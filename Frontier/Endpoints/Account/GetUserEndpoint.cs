using Core.Boundaries;
using FastEndpoints;
using Frontier.Contracts.Responses;
using CrazyLizard.Contracts.Requests;
using CrazyLizard.Shared.SharedMappers;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Account
{
    public class GetNotificationPreferencesEndpoint(IAccountService accountService) : Endpoint<IdRequest, UserDTO, UserResponseMapper>
    {
        public override void Configure()
        {
            Get("/account/{id}");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            CoreUser userShard = await accountService.GetCoreUserAsync(request.Id);

            if (userShard == null)
                await Send.NotFoundAsync(cancellationToken);

            await SendMappedAsync(userShard, 200, cancellationToken);
        }
    }
}