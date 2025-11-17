using CherAmiAPI.Interfaces;
using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;
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

    public class EmailAuthEndpoint(UserManager<User> userManager, ApplicationDbContext ctx, IKeyService keyService) : Endpoint<EmailAuthRequest>
    {
        public override void Configure()
        {
            Post("/auth/email");
            AllowAnonymous();
        }

        public override async Task HandleAsync(EmailAuthRequest request, CancellationToken cancellationToken)
        {
            User user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                user = new()
                {
                    UserName = request.Email,
                    Email = request.Email,
                };

                await userManager.CreateAsync(user);
            }

            if (user.Id != 7 && user.Id != 8)
            {
                Random random = new();
                string code = "";
                for (int i = 0; i < 6; i++)
                {
                    code = code + random.Next(0, 10).ToString();
                }

                ctx.EmailLogins.Add(new EmailLogin { Email = request.Email, Code = code, ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15) });
                await ctx.SaveChangesAsync(cancellationToken);

                using HttpClient client = new();
                client.DefaultRequestHeaders.Add("Authorization", $"key {await keyService.GetSecretAsync("OneSignal-API-Key")}");

                var body = new
                {
                    app_id = await keyService.GetSecretAsync("OneSignal-App-Id"),
                    template_id = "c0384ddb-1b48-4080-8b4f-f0edb769323a",
                    email_to = new string[] { user.Email },
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