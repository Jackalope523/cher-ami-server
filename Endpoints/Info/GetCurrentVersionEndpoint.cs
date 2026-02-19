using FastEndpoints;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Info
{
    public record VersionResponse
    {
        public string Version { get; set; }
    }

    public class GetCurrentVersionEndpoint() : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/version");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            VersionResponse response = new()
            {
                Version = "1.0.4",
            };

            await Send.OkAsync(response, cancellationToken);
        }
    }
}