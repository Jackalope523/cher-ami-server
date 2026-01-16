using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Exceptions;
using CherAmiAPI.Interfaces;
using FastEndpoints;
using FastEndpoints.Security;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
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

    public class EmailVerifyEndpoint(UserManager<User> userManager, ApplicationDbContext ctx, IKeyService keyService) : Endpoint<EmailVerifyRequest>
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

                if (user == null)
                {
                    user = new()
                    {
                        ExternalId = Guid.NewGuid(),
                        UserName = request.Email,
                        Email = request.Email,
                        EmailConfirmed = true,
                        JoinDate = DateTimeOffset.UtcNow
                    };

                    await userManager.CreateAsync(user);
                }

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