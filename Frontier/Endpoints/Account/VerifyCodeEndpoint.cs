using Core.Boundaries;
using CrazyLizard.Boundaries.Service;
using CrazyLizard.Entities;
using CrazyLizard.Exceptions;
using FastEndpoints;
using FastEndpoints.Security;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ValidationException = CrazyLizard.Exceptions.ValidationException;

namespace CrazyLizard.Endpoints.Account
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

    public class VerifyCodeEndpoint(UserManager<User> userManager, IAccountService accountService, IEmailService emailService, IKeyService keyService) : Endpoint<VerifyLoginRequest>
    {
        public override void Configure()
        {
            Post("/account/verify");
            AllowAnonymous();
        }

        public async Task<bool> CheckStaticCode(long userId, string code)
        {
            if (!(userId == 2 || userId == 7 || userId == 8))
            { return false; }

            string staticCode = userId switch
            {
                2 => "600613",
                7 => "449913",
                8 => "600613",
                _ => throw new ValidationException($"Invalid code {code} for user {userId}.")
            };

            return !string.IsNullOrEmpty(staticCode) && code.Equals(staticCode);
        }

        public override async Task HandleAsync(VerifyLoginRequest request, CancellationToken cancellationToken)
        {
            var user = await accountService.GetCoreUserAsync(request.PhoneNumber);

            if (await userManager.IsLockedOutAsync(user))
            {
                throw new LockedOutException($"User {user.Id}'s account is locked.");
            }

            #region UNSAFE — MODIFICATION AUTHORISATION FROM CHRONOS REQUIRED
            // Check if development environment or special account
            if (user.Id == 2 || user.Id == 7 || user.Id == 8)
            {
                // Verify static code
                if (await CheckStaticCode(user.Id, request.Code))
                {
                    //await signInManager.SignInAsync(user, false);

                    string jwtToken = JwtBearer.CreateToken(
                    o =>
                    {
                        o.SigningKey = "b10fa28c-9390-45a1-88b7-dff66ae71e0c";
                        o.ExpireAt = DateTime.UtcNow.AddDays(1);
                        o.User.Claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                        o.User.Claims.Add(new Claim("PhoneNumber", user.PhoneNumber));
                    });

                    await Send.OkAsync(new { Token = jwtToken, user.PhoneNumber }, cancellationToken);
                    return;
                }
                else
                {
                    throw new AuthenticationException($"OTP code {request.Code} is invalid.");
                }
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



                    //await signInManager.SignInAsync(user, false);

                    var jwtToken = JwtBearer.CreateToken(
                    o =>
                    {
                        o.SigningKey = "b10fa28c-9390-45a1-88b7-dff66ae71e0c";
                        o.ExpireAt = DateTime.UtcNow.AddDays(1);
                        o.User.Claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                        o.User.Claims.Add(new Claim("PhoneNumber", user.PhoneNumber));
                    });

                    await Send.OkAsync(new { Token = jwtToken }, cancellationToken);
                }
                else
                {
                    await userManager.AccessFailedAsync(user);
                    throw new AuthenticationException($"OTP code {request.Code} is invalid.");
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
                    //await signInManager.SignInAsync(user, false);

                    var jwtToken = JwtBearer.CreateToken(
                    o =>
                    {
                        o.SigningKey = "b10fa28c-9390-45a1-88b7-dff66ae71e0c";
                        o.ExpireAt = DateTime.UtcNow.AddDays(1);
                        o.User.Claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                        o.User.Claims.Add(new Claim("PhoneNumber", user.PhoneNumber));
                    });

                    await Send.OkAsync(new { Token = jwtToken }, cancellationToken);


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
                    throw new AuthenticationException($"OTP code {request.Code} is invalid.");
                }
            }
        }
    }
}