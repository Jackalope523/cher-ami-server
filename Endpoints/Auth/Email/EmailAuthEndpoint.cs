using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Services;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Auth.Email
{
    public class EmailAuthRequest
    {
        public string Email { get; set; }
    }

    public class EmailAuthRequestValidator : Validator<EmailAuthRequest>
    {
        public EmailAuthRequestValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email must be valid.")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));
        }
    }

    public class EmailAuthEndpoint(IConfiguration config, UserManager<User> userManager, OneSignalService oneSignalService, ApplicationDbContext ctx, IKeyService keyService) : Endpoint<EmailAuthRequest>
    {
        public override void Configure()
        {
            Post("/auth/email");
            AllowAnonymous();
        }

        public override async Task HandleAsync(EmailAuthRequest request, CancellationToken cancellationToken)
        {
            Task<string> appleReviewEmail = keyService.GetSecretAsync("Apple-Review-Email");
            Task<string> googleReviewEmail = keyService.GetSecretAsync("Google-Review-Email");
            if (request.Email != await appleReviewEmail && request.Email != await googleReviewEmail)
            {
                Random random = new();
                string code = "";
                for (int i = 0; i < 6; i++)
                {
                    code = code + random.Next(0, 10).ToString();
                }

                ctx.EmailLogins.Add(new EmailLogin { Email = request.Email, Code = code, ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15) });

                User user = await userManager.FindByEmailAsync(request.Email);

                if (user == null)
                {
                    user = new()
                    {
                        UserName = request.Email,
                        Email = request.Email,
                        AccountStatus = UserAccountStatus.Prospective,
                    };

                    await userManager.CreateAsync(user);
                }

                if (user.ExternalId == default)
                {
                    user.ExternalId = Guid.NewGuid();
                }

                if (user.OneSignalId == default)
                {
                    user.OneSignalId = await oneSignalService.CreateUserAsync(user.ExternalId, user.Email, cancellationToken);
                }

                await ctx.SaveChangesAsync(cancellationToken);

                await oneSignalService.SendTemplatedEmailAsync(
                    config["ONESIGNAL_VERIFY_EMAIL_TEMPLATE_ID"],
                    [request.Email],
                    customData: new { code },
                    includeUnsubscribed: true,
                    cancellationToken: cancellationToken);
            }

            await Send.NoContentAsync(cancellationToken);
        }
    }
}