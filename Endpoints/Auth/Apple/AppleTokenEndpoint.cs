using CherAmiAPI.Endpoints.Users;
using CherAmiAPI.Entities;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Services;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using FastEndpoints.Security;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Stripe;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
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

    public class AppleTokenEndpoint(UserManager<User> userManager, IKeyService keyService, HttpClient httpClient, CustomerService customerService, OneSignalService oneSignalService) : Endpoint<AppleTokenRequest>
    {
        public override void Configure()
        {
            Post("/auth/apple/token");
            AllowAnonymous();
        }

        public override async Task HandleAsync(AppleTokenRequest request, CancellationToken cancellationToken)
        {
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
            bool onboarded = false;

            if (user == null)
            {
                user = new()
                {
                    ExternalId = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    EmailConfirmed = email_verified,
                    AppleId = sub,
                    JoinDate = DateTimeOffset.UtcNow
                };

                user.OneSignalId = await oneSignalService.CreateUserAsync(user.ExternalId, user.Email);

                var options = new CustomerCreateOptions
                {
                    Name = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                };

                Customer customer = await customerService.CreateAsync(options, cancellationToken: cancellationToken);
                user.StripeCustomerId = customer.Id;

                await userManager.CreateAsync(user);
            }
            else
            {
                // User was partially created in Apple Callback Endpoint.
                if (user.ExternalId == Guid.Empty)
                {
                    user.ExternalId = Guid.NewGuid();
                    user.EmailConfirmed = email_verified;
                    user.AppleId = sub;
                    user.JoinDate = DateTimeOffset.UtcNow;
                    user.OneSignalId = await oneSignalService.CreateUserAsync(user.ExternalId, user.Email);

                    var options = new CustomerCreateOptions
                    {
                        Name = $"{user.FirstName} {user.LastName}",
                        Email = user.Email,
                    };

                    Customer customer = await customerService.CreateAsync(options, cancellationToken: cancellationToken);
                    user.StripeCustomerId = customer.Id;

                    await userManager.UpdateAsync(user);
                } 
                // User was created via some other sign in but is using Apple now.
                else if (user.AppleId == null) {
                    user.AppleId = sub;
                    await userManager.UpdateAsync(user);
                }
                
                onboarded = user.FirstName != null && user.LastName != null;
            }       

            string signingKey = await keyService.GetSecretAsync("Cher-Ami-API-Signing-Key");
            string jwtToken = JwtBearer.CreateToken(
                o =>
                {
                    o.SigningKey = signingKey;
                    o.ExpireAt = DateTime.UtcNow.AddDays(10);
                    o.User.Claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                    o.User.Claims.Add(new Claim("Email", user.Email));
                });

            await Send.OkAsync(new { Token = jwtToken, Onboarded = onboarded }, cancellationToken);
        }
    }
}