using Core.Boundaries;
using FastEndpoints;
using Frontier.Contracts.Requests;
using LazyLizardBackend.Contracts.Requests;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;

namespace Frontier.Endpoints.Account
{
    public class VerifyCode(SignInManager<CoreUser> signInManager, UserManager<CoreUser> userManager, IAccountService accountService, IEmailService emailService) : Endpoint<VerifyLoginRequest>
    {
        public override void Configure()
        {
            Post("/account/verify");
            AllowAnonymous();
        }

        public override async Task HandleAsync(VerifyLoginRequest request, CancellationToken cancellationToken)
        {
            var user = await accountService.GetCoreUserAsync(request.PhoneNumber);

            if (await userManager.IsLockedOutAsync(user))
            {
                throw new UserErrorException(AccountErrorCode.LOCKED_OUT);
            }

            #region UNSAFE — MODIFICATION AUTHORISATION FROM CHRONOS REQUIRED
            // Check if development environment or special account
            if (bypass.IsGlobalBypassEnabled())
            {
                var code = await userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
                await userManager.ChangePhoneNumberAsync(user, user.PhoneNumber, code);
                await signInManager.SignInAsync(user, false);
                return;
            }
            else if (bypass.IsClassifiedAccount(user.Id))
            {
                // Verify static code
                if (bypass.IsGlobalBypassEnabled() || bypass.CheckStaticCode(user.Id, request.Code))
                {
                    await signInManager.SignInAsync(user, false);
                    return;
                }
                else
                { throw new UserErrorException(AccountErrorCode.INCORRECT_CODE); }
            }
            #endregion

            // Check if the account is activated
            if (await userManager.IsPhoneNumberConfirmedAsync(user))
            {
                // Account is activated, check 2FA token validity
                var result = await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider, request.Code);
                if (result)
                {
                    // Token matched, reset access tries and sign user in
                    await userManager.ResetAccessFailedCountAsync(user);
                    await signInManager.SignInAsync(user, false);
                }
                else
                {
                    await userManager.AccessFailedAsync(user);
                    throw new UserErrorException(AccountErrorCode.INCORRECT_CODE);
                }
            }
            else
            {
                // Account is not activated, check change number token validity
                var result = await userManager.ChangePhoneNumberAsync(user, user.PhoneNumber, request.Code);
                if (result.Succeeded)
                {
                    // Token matched, reset access tries and sign user in
                    await userManager.ResetAccessFailedCountAsync(user);
                    await signInManager.SignInAsync(user, false);

                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        // Send verification email if an email is added
                        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                        var confirmationLink = Url.Action("email", "account", new { token, email = user.Email }, Request.Scheme);
                        await emailService.SendEmailAsync(user.Email, "Welcome to CANARY!", $"Verify your CANARY email.\n\n{confirmationLink}");
                    }
                }
                else
                {
                    await userManager.AccessFailedAsync(user);
                    throw new UserErrorException(AccountErrorCode.INCORRECT_CODE);
                }
            }

            await Send.NoContentAsync(cancellationToken);
        }
    }
}