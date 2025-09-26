using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Auth.Google
{
    public class GoogleAuthEndpoint() : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/auth/google");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            Dictionary<string, string> queryParams = new()
            {
                ["client_id"] = "JACKALOPE PUT REAL CLIENT ID HERE",
                ["response_type"] = "code",
                ["scope"] = "openid email",
                ["redirect_uri"] = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/auth/google/callback",
                ["state"] = RandomNumberGenerator.GetString(['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'], 30),
            };

            string redirectUrl = QueryHelpers.AddQueryString("https://accounts.google.com/o/oauth2/v2/auth", queryParams);

            await Send.RedirectAsync(redirectUrl);
        }
    }
}