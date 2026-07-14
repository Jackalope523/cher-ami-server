using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Services;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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

    public class EmailAuthEndpoint(IConfiguration config, UserManager<User> userManager, OneSignalService oneSignalService, ApplicationDbContext ctx, IKeyService keyService, IHttpClientFactory httpClientFactory) : Endpoint<EmailAuthRequest>
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
                    await oneSignalService.AddTagAsync(user.ExternalId, "email_reminders", "1", cancellationToken);
                    await oneSignalService.AddTagAsync(user.ExternalId, "email_marketing", "1", cancellationToken);
                }

                await ctx.SaveChangesAsync(cancellationToken);

                HttpClient client = httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Authorization", $"key {await keyService.GetSecretAsync("OneSignal-API-Key")}");

                var body = new
                {
                    app_id = config["ONESIGNAL_APP_ID"],
                    template_id = config["ONESIGNAL_VERIFY_EMAIL_TEMPLATE_ID"],
                    email_to = new[] { request.Email },
                    custom_data = new { code },
                    include_unsubscribed = true,
                };

                using StringContent jsonBody = new(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

                using HttpResponseMessage response = await client.PostAsync("https://api.onesignal.com/notifications?c=email", jsonBody, cancellationToken);
                response.EnsureSuccessStatusCode();
            }

            await Send.NoContentAsync(cancellationToken);
        }
    }
}