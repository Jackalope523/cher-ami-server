using FastEndpoints;
using LazyLizardBackend.Contracts.Requests;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace LazyLizardBackend.Endpoints.Profile
{
    public class UnblockUser(IProfileService profileService) : Endpoint<IdRequest>
    {
        public override void Configure()
        {
            Delete("/account/{id}/block");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await profileService.UnblockUserAsync(userId, request.Id);
            await Send.NoContentAsync(cancellationToken);
        }
    }
}
