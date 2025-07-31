using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class GetAccount(UserManager<CoreUser> userManager) : EndpointWithoutRequest<AccountShard>
    {
        public override void Configure()
        {
            Get("/account");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            CoreUser user = await userManager.GetUserAsync(HttpContext.User);

            AccountShard toReturn = new
            (
                user.Id,
                user.PhoneNumber,
                user.Email,
                user.Title,
                user.GivenName,
                user.FamilyName,
                user.DateOfBirth,
                user.IsPhoneConfirmed,
                user.IsEmailConfirmed,
                user.AccountStatus,
                user.JoinDate,
                user.TimeOfUserAgreement,
                user.NotificationId

            );

            await Send.OkAsync(toReturn, ct);
        }
    }
}
