using CrazyLizard.Entities;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Users
{
    public class GetLastUserAgreementEndpoint(UserManager<User> userManager) : EndpointWithoutRequest<DateTimeOffset>
    {
        public override void Configure()
        {
            Get("/account/agreement");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            //User user = await userManager.GetUserAsync(HttpContext.User);

            //await Send.OkAsync(user.TimeOfUserAgreement, cancellationToken);
            throw new NotImplementedException();
        }
    }
}