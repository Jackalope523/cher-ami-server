using CherAmiAPI.Contexts;
using CherAmiAPI.Endpoints.Users;
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
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Auth.Apple
{
    public class AppleTokenRequest
    {
        public string AuthorizationCode { get; set; }
    }

    public class AppleTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
    }

    public class AppleTokenRequestValidator : Validator<AppleTokenRequest>
    {
        public AppleTokenRequestValidator()
        {
            RuleFor(x => x.AuthorizationCode)
                .NotEmpty().WithMessage("Authorization code token is required.");
        }
    }

    public class AppleTokenEndpoint(UserManager<User> userManager, ApplicationDbContext ctx, IKeyService keyService, IHttpClientFactory httpClientFactory, CustomerService customerService, OneSignalService oneSignalService, INameService nameService, CircleService circleService) : Endpoint<AppleTokenRequest>
    {
        public override void Configure()
        {
            Post("/auth/apple/token");
            AllowAnonymous();
        }

        public override async Task HandleAsync(AppleTokenRequest request, CancellationToken cancellationToken)
        {
            HttpClient httpClient = httpClientFactory.CreateClient();
            DiscoveryDocument discoveryDocument = await httpClient.GetFromJsonAsync<DiscoveryDocument>("https://appleid.apple.com/.well-known/openid-configuration", cancellationToken: cancellationToken);

            var parameters = new Dictionary<string, string>
            {
                ["client_id"] = "com.hollowinc.cherami.api",
                ["client_secret"] = await keyService.GetSecretAsync("Apple-OAuth-Client-Secret"),
                ["code"] = request.AuthorizationCode,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/auth/apple/callback"
            };
            
            using FormUrlEncodedContent formContent = new(parameters);

            using HttpResponseMessage response = await httpClient.PostAsync(discoveryDocument.TokenEndpoint, formContent, cancellationToken);
            response.EnsureSuccessStatusCode();

            AppleTokenResponse content = await response.Content.ReadFromJsonAsync<AppleTokenResponse>(cancellationToken: cancellationToken);

            JwtSecurityToken idToken = new JwtSecurityTokenHandler().ReadJwtToken(content.IdToken);

            string email = idToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            string sub = idToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            bool email_verified = bool.Parse(idToken.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value);

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

            user.EmailConfirmed = email_verified;
            user.AppleId = sub;
            user.AccountStatus = UserAccountStatus.Active;

            if (user.FirstName == default)
            {
                user.FirstName = nameService.GetRandomFirstName();
            }
            if (user.LastName == default)
            {
                user.LastName = nameService.GetRandomLastName();
            }
            if (user.ExternalId == default)
            {
                user.ExternalId = Guid.NewGuid();
            }
            if (user.TimeOfUserAgreement == default)
            {
                user.TimeOfUserAgreement = DateTimeOffset.UtcNow;
            }
            if (user.OneSignalId == default)
            {
                user.OneSignalId = await oneSignalService.CreateUserAsync(user.ExternalId, user.Email, cancellationToken);
                await oneSignalService.AddTagAsync(user.ExternalId, "email_reminders", "1", cancellationToken);
                await oneSignalService.AddTagAsync(user.ExternalId, "email_marketing", "1", cancellationToken);
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
               await circleService.CreateCircleAsync(user.Id, $"My Circle", cancellationToken: cancellationToken);
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
                });

            await Send.OkAsync(new { Token = jwtToken, Onboarded = user.FirstName != null && user.LastName != null }, cancellationToken);
        }
    }
}