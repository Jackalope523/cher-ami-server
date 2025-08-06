using FastEndpoints;
using Frontier.Contracts.Requests;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace LazyLizardBackend.Endpoints.Circle
{
    public class EditCircle(ICircleService circles) : Endpoint<CircleEditManifest>
    {
        public override void Configure()
        {
            Post("/circle/{circleId}/edit");
            AllowFileUploads();
        }

        public override async Task HandleAsync(CircleEditManifest request, CancellationToken cancellationToken)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var circleId = Route<long>("circleId");

            using var stream = new MemoryStream();
            if (request.Image is { Length: > 0 })
                await request.Image.CopyToAsync(stream, cancellationToken);

            await circles.EditCircleAsync(
                userId,
                circleId,
                title: request.Title,
                plan: request.Plan,
                schedule: request.Schedule,
                header: stream
            );

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
