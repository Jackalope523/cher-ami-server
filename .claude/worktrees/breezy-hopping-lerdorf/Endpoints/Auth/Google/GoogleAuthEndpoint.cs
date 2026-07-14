using CherAmiAPI.Interfaces;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Auth.Google
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
            using HttpClient httpClient = new();
            DiscoveryDocument discoveryDocument = await httpClient.GetFromJsonAsync<DiscoveryDocument>("https://accounts.google.com/.well-known/openid-configuration", cancellationToken: cancellationToken);

            Dictionary<string, string> queryParams = new()
            {
                ["client_id"] = await keyService.GetSecretAsync("Google-OAuth-Client-Id"),
                ["response_type"] = "code",
                ["scope"] = "openid email profile",
                ["redirect_uri"] = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/auth/google/callback",
                ["state"] = request.State,
            };

            string redirectUrl = QueryHelpers.AddQueryString(discoveryDocument.AuthorizationEndpoint, queryParams);

            await Send.RedirectAsync(redirectUrl, true, true);
        }
    }
}