using CrazyLizard.Entities;
using CrazyLizard.Interfaces.Service;
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
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Auth.Google
{
    public class GoogleTokenRequest
    {
        public string AuthorizationCode { get; set; }
    }

    public class GoogleTokenResponse
    {
        public string Access_Token { get; set; }
        public int Expires_In { get; set; }
        public string Id_Token { get; set; }
        public string Scope { get; set; }
        public string Token_Type { get; set; }
        public string Refresh_Token { get; set; }
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
            using HttpClient client = new();

            var body = new
            {
                code = request.AuthorizationCode,
                client_id = await keyService.GetSecretAsync("Google-OAuth-Client-Id"),
                client_secret = await keyService.GetSecretAsync("Google-OAuth-Client-Secret"),
                redirect_uri = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/auth/google/callback",
                grant_type = "authorization_code"
            };

            using StringContent jsonBody = new(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await client.PostAsync("https://oauth2.googleapis.com/token", jsonBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            GoogleTokenResponse content = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken: cancellationToken);

            JwtSecurityToken idToken = new JwtSecurityTokenHandler().ReadJwtToken(content.Id_Token);

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
                onboarded = user.FirstName != null && user.LastName != null && user.AvatarPath != null && user.CircleId != null;
            }

            string jwtToken = JwtBearer.CreateToken(
                o =>
                {
                    // JACKALOPE: This needs to be in secure store.
                    o.SigningKey = "b10fa28c-9390-45a1-88b7-dff66ae71e0c";
                        o.ExpireAt = DateTime.UtcNow.AddDays(1);
                        o.User.Claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                        o.User.Claims.Add(new Claim("Email", user.Email));
                });

            await Send.OkAsync(new { Token = jwtToken, Onboarded = onboarded }, cancellationToken);
        }
    }
}