using Core.Boundaries;
using FastEndpoints;
using CrazyLizard.Shared.SharedMappers;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Entities;
using CrazyLizard.Shared.Responses;

namespace Frontier.Endpoints.Account
{
    public class GetAccountEndpoint(UserManager<User> userManager) : EndpointWithoutRequest<AccountDTO, AccountResponseMapper>
    {
        public override void Configure()
        {
            Get("/account");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            User user = await userManager.GetUserAsync(HttpContext.User);
            await SendMapped(user, 200, cancellationToken);
        }
    }
}
