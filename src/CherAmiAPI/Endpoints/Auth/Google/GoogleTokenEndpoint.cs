using CherAmiAPI.Interfaces;
using CherAmiAPI.Services;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using FluentValidation;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
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

    public class GoogleTokenEndpoint(IKeyService keyService, IHttpClientFactory httpClientFactory, AuthService authService) : Endpoint<GoogleTokenRequest>
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

            (string token, bool onboarded) = await authService.LoginWithGoogleAsync(email, sub, email_verified, firstName, lastName, cancellationToken);

            await Send.OkAsync(new { Token = token, Onboarded = onboarded }, cancellationToken);
        }
    }
}
