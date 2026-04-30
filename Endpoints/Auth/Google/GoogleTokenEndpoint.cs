using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Services;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using FastEndpoints.Security;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Stripe;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Auth.Google
{
    public class GoogleTokenRequest
    {
        public string AuthorizationCode { get; set; }
    }

    public class GoogleTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
    }

    public class GoogleTokenRequestValidator : Validator<GoogleTokenRequest>
    {
        public GoogleTokenRequestValidator()
        {
            RuleFor(x => x.AuthorizationCode)
                .NotEmpty().WithMessage("Authorization code token is required.");
        }
    }

    public class GoogleTokenEndpoint(UserManager<User> userManager, ApplicationDbContext ctx, IKeyService keyService, IHttpClientFactory httpClientFactory, CustomerService customerService, OneSignalService oneSignalService, CircleService circleService) : Endpoint<GoogleTokenRequest>
    {
        public override void Configure()
        {
            Post("/auth/google/token");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GoogleTokenRequest request, CancellationToken cancellationToken)
        {
            HttpClient httpClient = httpClientFactory.CreateClient();
            DiscoveryDocument discoveryDocument = await httpClient.GetFromJsonAsync<DiscoveryDocument>("https://accounts.google.com/.well-known/openid-configuration", cancellationToken: cancellationToken);

            var body = new
            {
                code = request.AuthorizationCode,
                client_id = await keyService.GetSecretAsync("Google-OAuth-Client-Id"),
                client_secret = await keyService.GetSecretAsync("Google-OAuth-Client-Secret"),
                redirect_uri = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/auth/google/callback",
                grant_type = "authorization_code"
            };

            using JsonContent jsonBody = JsonContent.Create(body);

            using HttpResponseMessage response = await httpClient.PostAsync(discoveryDocument.TokenEndpoint, jsonBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            GoogleTokenResponse content = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken: cancellationToken);

            JwtSecurityToken idToken = new JwtSecurityTokenHandler().ReadJwtToken(content.IdToken);

            string email = idToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            string sub = idToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            bool email_verified = bool.Parse(idToken.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value);
            string firstName = idToken.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;
            string lastName = idToken.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value;

            User user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new()
                {
                    UserName = email,
                    Email = email,
                };

                await userManager.CreateAsync(user);
            }

            if (user.OneSignalId == default)
            {
                user.OneSignalId = await oneSignalService.CreateUserAsync(user.ExternalId, user.Email, cancellationToken);
            }
            if (user.AccountStatus == UserAccountStatus.Prospective)
            {
                await oneSignalService.AddTagAsync(user.ExternalId, "email_reminders", "1", cancellationToken);
                await oneSignalService.AddTagAsync(user.ExternalId, "email_marketing", "1", cancellationToken);
            }

            user.EmailConfirmed = email_verified;
            user.GoogleId = sub;
            user.FirstName = firstName ?? "";
            user.LastName = lastName ?? "";
            user.AccountStatus = UserAccountStatus.Active;

            if (user.ExternalId == default)
            {
                user.ExternalId = Guid.NewGuid();
            }
            if (user.TimeOfUserAgreement == default)
            {
                user.TimeOfUserAgreement = DateTimeOffset.UtcNow;
            }
            if (user.JoinDate == default)
            {
                user.JoinDate = DateTimeOffset.UtcNow;
                await oneSignalService.AddTagAsync(user.ExternalId, "joined_at", user.JoinDate.ToUnixTimeSeconds().ToString(), cancellationToken);
            }
            if (user.StripeCustomerId == default)
            {
                var options = new CustomerCreateOptions
                {
                    Name = $"{user.FirstName} {user.LastName}",
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
    }
}