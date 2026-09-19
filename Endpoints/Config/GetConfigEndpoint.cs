using FastEndpoints;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Info
{
    public record ConfigResponse
    {
        // Deprecated. Older builds compared this for equality; its value is frozen so
        // that changing it can never wall them. New builds read MinimumVersion.
        public string Version { get; set; }
        public string MinimumVersion { get; set; }
        public string OneSignalAppId { get; set; }
        public string StripePublishableKey { get; set; }
    }

    public class GetConfigEndpoint(IConfiguration config) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/config");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            ConfigResponse response = new()
            {
                Version = "1.0.5",
                MinimumVersion = "1.0.10",
                OneSignalAppId = config["ONESIGNAL_APP_ID"],
                StripePublishableKey = config["STRIPE_PUBLISHABLE_KEY"]
            };

            await Send.OkAsync(response, cancellationToken);
        }
    }
}