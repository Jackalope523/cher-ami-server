using CherAmiAPI.Interfaces;
using CherAmiAPI.Shared.Responses;
using CherAmiAPI.Entities;
using FastEndpoints;
using FastEndpoints.Security;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System;
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

    public class GoogleTokenEndpoint(UserManager<User> userManager, IKeyService keyService) : Endpoint<GoogleTokenRequest>
    {
        public override void Configure()
        {
            Post("/auth/google/token");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GoogleTokenRequest request, CancellationToken cancellationToken)
        {
            using HttpClient httpClient = new();
            DiscoveryDocument discoveryDocument = await httpClient.GetFromJsonAsync<DiscoveryDocument>("https://accounts.google.com/.well-known/openid-configuration", cancellationToken: cancellationToken);

            var body = new
            {
                code = request.AuthorizationCode,
                client_id = await keyService.GetSecretAsync("Google-OAuth-Client-Id"),
                client_secret = await keyService.GetSecretAsync("Google-OAuth-Client-Secret"),
                redirect_uri = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/auth/google/callback",
                grant_type = "authorization_code"
            };

            using StringContent jsonBody = new(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await httpClient.PostAsync(discoveryDocument.TokenEndpoint, jsonBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            GoogleTokenResponse content = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken: cancellationToken);

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
                    GoogleId = sub,
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
    }
}