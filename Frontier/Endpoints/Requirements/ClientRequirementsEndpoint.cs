using FastEndpoints;
using System.Threading;
using System.Threading.Tasks;

namespace LazyLizardBackend.Endpoints.Requirments
{
    public class ClientDetailsDTO
    {
        public string MinimumVersion { get; set; }
        public string ServerVersion { get; set; }
        public int PageSize { get; set; }
    }

    public class ClientRequirementsEndpoint : EndpointWithoutRequest<ClientDetailsDTO>
    {
        public override void Configure()
        {
            Get("/requirements");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            ClientDetailsDTO response = new()
            {
                MinimumVersion = "0.0.0",
                ServerVersion = "0.0.0",
                PageSize = 10,
            };

            await Send.OkAsync(response, cancellationToken);
        }
    }
}
