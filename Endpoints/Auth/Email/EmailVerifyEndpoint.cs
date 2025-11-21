using CherAmiAPI.Contexts;
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
            string appleEmail = await keyService.GetSecretAsync("Apple-Review-Email");
            string googleEmail = await keyService.GetSecretAsync("Google-Review-Email");

            string appleCode = await keyService.GetSecretAsync("Apple-Review-Code");
            string googleCode = await keyService.GetSecretAsync("Google-Review-Code");

            bool isValid;
            if (request.Email == appleEmail || request.Email == googleEmail)
            {
                if (request.Email == appleEmail && request.Code == appleCode) isValid = true;
                else if (request.Email == googleEmail && request.Code == googleCode) isValid = true;
                else isValid = false;
            }
            else
            {
                isValid = await ctx.EmailLogins.AnyAsync(x => x.Email == request.Email && x.Code == request.Code && DateTimeOffset.UtcNow < x.ExpiresAt, cancellationToken: cancellationToken);
            }

            if (isValid)
            {
                User user = await ctx.Users.Where(x => x.Email == request.Email).SingleAsync(cancellationToken: cancellationToken);
                bool onboarded = false;

                if (user == null)
                {
                    user = new()
                    {
                        UserName = request.Email,
                        Email = request.Email,
                        EmailConfirmed = true,
                    };

                    await userManager.CreateAsync(user);
                }
                else
                {
                    onboarded = user.FirstName != null && user.LastName != null && user.AvatarPath != null;
                }

                string signingKey = await keyService.GetSecretAsync("Cher-Ami-API-Signing-Key");
                string jwtToken = JwtBearer.CreateToken(
                    o =>
                    {
                        o.SigningKey = signingKey;
                        o.ExpireAt = DateTime.UtcNow.AddDays(1);
                        o.User.Claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                        o.User.Claims.Add(new Claim("Email", user.Email));
                    }
                );

                await Send.OkAsync(new { Token = jwtToken, Onboarded = onboarded }, cancellationToken);
            }
            else
            {
                throw new AuthenticationException();
            }
        }
    }
}