using CherAmiAPI.Endpoints.Info;
using FastEndpoints;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Config
{
    public record VersionResponse
    {
        public string Version { get; set; }
    }

    public class GetVersionEndpoint(IConfiguration config) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/version");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            ConfigResponse response = new()
            {
                Version = "1.0.4",
            };

            await Send.OkAsync(response, cancellationToken);
        }
    }
}