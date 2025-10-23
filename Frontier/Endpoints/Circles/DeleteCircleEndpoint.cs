using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Interfaces.Service;
using CrazyLizard.Shared.Requests;

namespace CrazyLizard.Endpoints.Circles
{
    public class Delete(ICircleService circles) : Endpoint<IdRequest>
    {
        public override void Configure()
        {
            Delete("/circle/{circleId}");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await circles.DeleteCircleAsync(userId, request.Id);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}