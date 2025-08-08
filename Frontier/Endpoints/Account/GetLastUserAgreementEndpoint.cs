using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class GetLastUserAgreementEndpoint(UserManager<CoreUser> userManager) : EndpointWithoutRequest<DateTimeOffset>
    {
        public override void Configure()
        {
            Get("/account/agreement");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            CoreUser user = await userManager.GetUserAsync(HttpContext.User);

            await Send.OkAsync(user.TimeOfUserAgreement, cancellationToken);
        }
    }
}