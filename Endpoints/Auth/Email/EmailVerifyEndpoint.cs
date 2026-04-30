using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Exceptions;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Services;
using FastEndpoints;
using FastEndpoints.Security;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using User = CherAmiAPI.Entities.User;

namespace CherAmiAPI.Endpoints.Auth.Email
{
    public class EmailVerifyRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }

    public class EmailVerifyRequestValidator : Validator<EmailVerifyRequest>
    {
        public EmailVerifyRequestValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email must be valid.")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required.")
                .MaximumLength(6).WithMessage("Code cannot exceed 6 characters.");
        }
    }

    public class EmailVerifyEndpoint(UserManager<User> userManager, ApplicationDbContext ctx, IKeyService keyService, CustomerService customerService, OneSignalService oneSignalService, INameService nameService, CircleService circleService) : Endpoint<EmailVerifyRequest>
    {
        public override void Configure()
        {
            Post("/auth/email/verify");
            AllowAnonymous();
        }

        public override async Task HandleAsync(EmailVerifyRequest request, CancellationToken cancellationToken)
        {
            Task<string> appleReviewEmail = keyService.GetSecretAsync("Apple-Review-Email");
            Task<string> googleReviewEmail = keyService.GetSecretAsync("Google-Review-Email");
            Task<string> appleReviewCode = keyService.GetSecretAsync("Apple-Review-Code");
            Task<string> googleReviewCode = keyService.GetSecretAsync("Google-Review-Code");

            bool isValid;
            if (request.Email == await appleReviewEmail || request.Email == await googleReviewEmail)
            {
                if (request.Email == await appleReviewEmail && request.Code == await appleReviewCode) isValid = true;
                else if (request.Email == await googleReviewEmail && request.Code == await googleReviewCode) isValid = true;
                else isValid = false;
            }
            else
            {
                isValid = await ctx.EmailLogins.AnyAsync(x => x.Email == request.Email && x.Code == request.Code && DateTimeOffset.UtcNow < x.ExpiresAt, cancellationToken: cancellationToken);
            }

            if (isValid)
            {
                User user = await userManager.FindByEmailAsync(request.Email);

                if (user.AccountStatus == UserAccountStatus.Prospective)
                {
                    await oneSignalService.AddTagAsync(user.ExternalId, "email_reminders", "1", cancellationToken);
                    await oneSignalService.AddTagAsync(user.ExternalId, "email_marketing", "1", cancellationToken);
                }

                user.EmailConfirmed = true;
                user.AccountStatus = UserAccountStatus.Active;


                if (user.FirstName == default)
                {
                    user.FirstName = nameService.GetRandomFirstName();
                }
                if (user.LastName == default)
                {
                    user.LastName = nameService.GetRandomLastName();
                }
                if (user.JoinDate == default)
                {
                    user.JoinDate = DateTimeOffset.UtcNow;
                    await oneSignalService.AddTagAsync(user.ExternalId, "joined_at", user.JoinDate.ToUnixTimeSeconds().ToString(), cancellationToken);
                }
                if (user.TimeOfUserAgreement == default)
                {
                    user.TimeOfUserAgreement = DateTimeOffset.UtcNow;
                }
                if (user.StripeCustomerId == default)
                {
                    var options = new CustomerCreateOptions
                    {
                        Email = user.Email,
                    };

                    Customer customer = await customerService.CreateAsync(options, cancellationToken: cancellationToken);
                    user.StripeCustomerId = customer.Id;
                }
                if (user.CircleId == default)
                {
                    await circleService.CreateCircleAsync(user.Id, $"{user.FirstName}'s Circle", cancellationToken: cancellationToken);
                }

                await ctx.SaveChangesAsync(cancellationToken);

                string signingKey = await keyService.GetSecretAsync("Cher-Ami-API-Signing-Key");
                string jwtToken = JwtBearer.CreateToken(
                    o =>
                    {
                        o.SigningKey = signingKey;
                        o.ExpireAt = DateTime.UtcNow.AddDays(10);
                        o.User.Claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                        o.User.Claims.Add(new Claim("Email", user.Email));
                    }
                );

                await Send.OkAsync(new { Token = jwtToken, Onboarded = user.FirstName != null && user.LastName != null }, cancellationToken);
            }
            else
            {
                throw new AuthenticationException();
            }
        }
    }
}