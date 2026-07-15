using CherAmiAPI.Services;
using CherAmiAPI.Shared.Requests;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Circles
{
    public class EditCircleHeaderEndpoint(CircleService circleService) : Endpoint<ImageRequest>
    {
        public override void Configure()
        {
            Post("/circle/header");
            AllowFileUploads();
        }

        public override async Task HandleAsync(ImageRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await circleService.UpdateHeaderAsync(userId, request.Image, cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
