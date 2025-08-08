using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class VerifyLoginRequest
    {
        public string PhoneNumber { get; set; }
        public string Code { get; set; }
    }

    public class VerifyLoginRequestValidator : Validator<VerifyLoginRequest>
    {
        public VerifyLoginRequestValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(20).WithMessage("Title cannot exceed 20 characters.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required.")
                .MaximumLength(6).WithMessage("Code cannot exceed 6 characters.");
        }
    }

    public class VerifyCodeEndpoint(SignInManager<CoreUser> signInManager, UserManager<CoreUser> userManager, IAccountService accountService, IEmailService emailService, IHostEnvironment environment, IKeyService keyService) : Endpoint<VerifyLoginRequest>
    {
        public override void Configure()
        {
            Post("/account/verify");
            AllowAnonymous();
        }

        public async Task<bool> CheckStaticCode(long userId, string code)
        {
            if (!(userId == -2 || userId == -7 || userId == -8))
            { return false; }

            string staticCode = userId switch
            {
                -2 => await keyService.GetClassifiedAccountCodeAsync(-7),
                -7 => await keyService.GetClassifiedAccountCodeAsync(-7),
                -8 => await keyService.GetClassifiedAccountCodeAsync(-8),
                _ => throw new UserErrorException(AccountErrorCode.NOT_FOUND)
            };

            return !string.IsNullOrEmpty(staticCode) && code.Equals(staticCode);
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
            if (!environment.IsProduction())
            {
                var code = await userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
                await userManager.ChangePhoneNumberAsync(user, user.PhoneNumber, code);
                await signInManager.SignInAsync(user, false);
                return;
            }
            else if (user.Id < 1)
            {
                // Verify static code
                if (!environment.IsProduction() || await CheckStaticCode(user.Id, request.Code))
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
                        string confirmationLink = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/account/email?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";
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