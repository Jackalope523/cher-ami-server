using CherAmiAPI.Interfaces;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Auth.Apple
{
    public class AppleAuthRequest
    {
        public string State { get; set; }
    }

    public class AppleAuthEndpoint() : Endpoint<AppleAuthRequest>
    {
        public override void Configure()
        {
            Get("/auth/apple");
            AllowAnonymous();
        }

        public override async Task HandleAsync(AppleAuthRequest request, CancellationToken cancellationToken)
        {
            using HttpClient httpClient = new();
            DiscoveryDocument discoveryDocument = await httpClient.GetFromJsonAsync<DiscoveryDocument>("https://appleid.apple.com/.well-known/openid-configuration", cancellationToken: cancellationToken);

            Dictionary<string, string> queryParams = new()
            {
                ["client_id"] = "com.hollowinc.cherami.api",
                ["response_type"] = "code",
                ["response_mode"] = "form_post",
                ["scope"] = "email name",
                ["redirect_uri"] = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/auth/apple/callback",
                ["state"] = request.State,
            };

            string redirectUrl = QueryHelpers.AddQueryString(discoveryDocument.AuthorizationEndpoint, queryParams);
            await Send.RedirectAsync(redirectUrl, true, true);
        }
    }
}