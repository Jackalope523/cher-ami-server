using FastEndpoints;
using CrazyLizard.Contracts.Requests;
using CrazyLizard.Shared.Responses;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Interfaces.Service;

namespace CrazyLizard.Endpoints.Media
{
    public class GetHeaderMetadataEndpoint(IMediaService mediaService) : Endpoint<IdRequest, ImageMetadataDTO>
    {
        public override void Configure()
        {
            Get("/media/headers/{id}/metadata");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            ImageMetadataDTO response = await mediaService.GetHeaderMetadataAsync(userId, request.Id);
            await Send.OkAsync(response, cancellationToken);
        }
    }
}