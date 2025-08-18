using Core.Boundaries;
using FastEndpoints;
using CrazyLizard.Contracts.Requests;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Profile
{
    public class BlockUser(IProfileService profileService) : Endpoint<IdRequest>
    {
        public override void Configure()
        {
            Post("/account/{id}/block");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await profileService.BlockUserAsync(userId, request.Id);
            await Send.NoContentAsync(cancellationToken);
        }
    }
}
