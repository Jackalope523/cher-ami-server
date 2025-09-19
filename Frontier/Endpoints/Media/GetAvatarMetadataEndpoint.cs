using FastEndpoints;
using CrazyLizard.Contracts.Requests;
using CrazyLizard.Shared.Responses;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Interfaces.Service;

namespace CrazyLizard.Endpoints.Media
{
    public class GetAvatarMetadataEndpoint(IMediaService mediaService) : Endpoint<IdRequest, ImageMetadataDTO>
    {
        public override void Configure()
        {
            Get("/media/avatars/{id}/metadata");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            ImageMetadataDTO response = await mediaService.GetAvatarMetadataAsync(userId, request.Id);
            await Send.OkAsync(response, cancellationToken);
        }
    }
}