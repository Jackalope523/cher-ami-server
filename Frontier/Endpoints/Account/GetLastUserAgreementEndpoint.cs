using Core.Boundaries;
using CrazyLizard.Entities;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class GetLastUserAgreementEndpoint(UserManager<User> userManager) : EndpointWithoutRequest<DateTimeOffset>
    {
        public override void Configure()
        {
            Get("/account/agreement");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            User user = await userManager.GetUserAsync(HttpContext.User);

            await Send.OkAsync(user.TimeOfUserAgreement, cancellationToken);
        }
    }
}