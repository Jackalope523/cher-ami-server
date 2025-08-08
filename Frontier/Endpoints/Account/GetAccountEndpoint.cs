using FastEndpoints;
using Frontier.Contracts.Responses;
using Mappers;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class GetAccountEndpoint(UserManager<CoreUser> userManager) : EndpointWithoutRequest<AccountDTO, AccountResponseMapper>
    {
        public override void Configure()
        {
            Get("/account");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            CoreUser user = await userManager.GetUserAsync(HttpContext.User);
            await SendMapped(user, 200, cancellationToken);
        }
    }
}
