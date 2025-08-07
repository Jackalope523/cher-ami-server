using FastEndpoints;
using LazyLizardBackend.Contracts.Requests;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace LazyLizardBackend.Endpoints.Account
{
    public class CreateAccount(UserManager<CoreUser> userManager, IAccountService accountService, ISMSService smsService) : Endpoint<CreateAccountRequest>
    {
        public override void Configure()
        {
            Post("/account/signup");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateAccountRequest request, CancellationToken cancellationToken)
        {
            var userExists = await accountService.GetUserExistsAsync(request.PhoneNumber);

            if (!userExists)
            {
                // Persist a new user
                await accountService.CreateUserAsync(request.PhoneNumber, request.Email,
                    request.Title, request.GivenName, request.FamilyName,
                    request.DateOfBirth.ToUniversalTime());

                // Send an SMS to new user with a generated change number token
                var user = await accountService.GetCoreUserAsync(request.PhoneNumber);
                var code = await userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
                await smsService.SendTextMessageAsync(user.PhoneNumber, $"Your Canary code is {code}");
            }
            else
            {
                // Account already exists
                var user = await accountService.GetCoreUserAsync(request.PhoneNumber);
                string code;

                // Login
                if (await userManager.IsPhoneNumberConfirmedAsync(user))
                {
                    code = await userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);
                }
                // Account is not activated, send an SMS with a generated change number token
                else
                {
                    code = await userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
                }

                await smsService.SendTextMessageAsync(user.PhoneNumber, $"Your Canary code is {code}");
            }

            await Send.NoContentAsync(cancellationToken);
        }
    }
}