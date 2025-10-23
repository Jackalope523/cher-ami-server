using CherAmiAPI.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Auth.Google
{
    public class GoogleAuthRequest
    {
        public string State { get; set; }
    }

    public class GoogleAuthEndpoint(IKeyService keyService) : Endpoint<GoogleAuthRequest>
    {
        public override void Configure()
        {
            Get("/auth/google");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GoogleAuthRequest request, CancellationToken cancellationToken)
        {
            Dictionary<string, string> queryParams = new()
            {
                ["client_id"] = await keyService.GetSecretAsync("Google-OAuth-Client-Id"),
                ["response_type"] = "code",
                ["scope"] = "openid email",
                ["redirect_uri"] = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/auth/google/callback",
                ["state"] = request.State,
            };

            string redirectUrl = QueryHelpers.AddQueryString("https://accounts.google.com/o/oauth2/v2/auth", queryParams);

            await Send.RedirectAsync(redirectUrl, true, true);
        }
    }
}