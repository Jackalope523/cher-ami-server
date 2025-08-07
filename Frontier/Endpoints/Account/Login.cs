using FastEndpoints;
using Frontier.Contracts.Requests;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class Login(UserManager<CoreUser> userManager, IAccountService accountService, ISMSService smsService) : Endpoint<LoginRequest>
    {
        public override void Configure()
        {
            Post("/account/login");
            AllowAnonymous();
        }

        public override async Task HandleAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            var user = await accountService.GetCoreUserAsync(request.PhoneNumber);

            #region UNSAFE — MODIFICATION AUTHORISATION FROM CHRONOS REQUIRED
            // Skip if bypass or classified
            if (bypass.IsGlobalBypassEnabled() ||
                bypass.IsClassifiedAccount(user.Id))
            { return; }
            #endregion

            string code;

            // Verify that the account is activated
            if (await userManager.IsPhoneNumberConfirmedAsync(user))
            {
                // Account is activated, generate regular 2FA token
                code = await userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);
            }
            else
            {
                // Account is not activated, generate change number token
                code = await userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
            }

            bool useWhatsApp = request.UseWhatsApp ?? false;

            // Send user code
            if (useWhatsApp)
            {
                await smsService.SendWhatsAppAuthMessageAsync(user.PhoneNumber, code);
            }
            else
            {
                await smsService.SendTextMessageAsync(user.PhoneNumber, $"Your Lazy Lizard code is {code}");
            }

            await Send.NoContentAsync(cancellationToken);
        }
    }
}