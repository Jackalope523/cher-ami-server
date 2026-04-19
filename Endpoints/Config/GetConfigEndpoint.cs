using FastEndpoints;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Info
{
    public record ConfigResponse
    {
        public string Version { get; set; }
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
                OneSignalAppId = config["ONESIGNAL_APP_ID"],
                StripePublishableKey = config["STRIPE_PUBLISHABLE_KEY"]
            };

            await Send.OkAsync(response, cancellationToken);
        }
    }
}