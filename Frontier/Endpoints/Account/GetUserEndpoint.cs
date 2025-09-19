using FastEndpoints;
using Frontier.Contracts.Responses;
using CrazyLizard.Contracts.Requests;
using CrazyLizard.Shared.SharedMappers;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Entities;
using CrazyLizard.Boundaries.Service;

namespace CrazyLizard.Endpoints.Account
{
    public class GetUserEndpoint(IAccountService accountService) : Endpoint<IdRequest, UserDTO, UserResponseMapper>
    {
        public override void Configure()
        {
            Get("/account/{id}");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            User user = await accountService.GetCoreUserAsync(request.Id);

            if (user == null)
                await Send.NotFoundAsync(cancellationToken);

            await SendMappedAsync(user, 200, cancellationToken);
        }
    }
}