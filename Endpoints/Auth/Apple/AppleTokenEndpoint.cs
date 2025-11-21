using CherAmiAPI.Interfaces;
using CherAmiAPI.Shared.Responses;
using CherAmiAPI.Entities;
using FastEndpoints;
using FastEndpoints.Security;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Serilog;
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

    public class AppleTokenEndpoint(UserManager<User> userManager, IKeyService keyService) : Endpoint<AppleTokenRequest>
    {
        public override void Configure()
        {
            Post("/auth/apple/token");
            AllowAnonymous();
        }

        public override async Task HandleAsync(AppleTokenRequest request, CancellationToken cancellationToken)
        {
            using HttpClient httpClient = new();
            DiscoveryDocument discoveryDocument = await httpClient.GetFromJsonAsync<DiscoveryDocument>("https://appleid.apple.com/.well-known/openid-configuration", cancellationToken: cancellationToken);

            var parameters = new Dictionary<string, string>
            {
                ["client_id"] = "com.hollowinc.cherami.api",
                ["client_secret"] = await keyService.GetSecretAsync("Apple-OAuth-Client-Secret"),
                ["code"] = request.AuthorizationCode,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/auth/apple/callback"
            };

            using var formContent = new FormUrlEncodedContent(parameters);

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
                    UserName = email,
                    Email = email,
                    EmailConfirmed = email_verified,
                    AppleId = sub,
                };

                await userManager.CreateAsync(user);
            }
            else
            {
                onboarded = user.FirstName != null && user.LastName != null;
            }

            string signingKey = await keyService.GetSecretAsync("Cher-Ami-API-Signing-Key");
            string jwtToken = JwtBearer.CreateToken(
                o =>
                {
                    o.SigningKey = signingKey;
                    o.ExpireAt = DateTime.UtcNow.AddDays(1);
                    o.User.Claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                    o.User.Claims.Add(new Claim("Email", user.Email));
                });

            await Send.OkAsync(new { Token = jwtToken, Onboarded = onboarded }, cancellationToken);
        }
    }
}