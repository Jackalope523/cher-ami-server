using FastEndpoints;
using Frontier.Contracts.Requests;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace LazyLizardBackend.Endpoints.Profile
{
    public class BlockUser(IProfileService profileService) : Endpoint<UserIdRequest>
    {
        public override void Configure()
        {
            Post("/account/{userId}/block");
        }

        public override async Task HandleAsync(UserIdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await profileService.BlockUserAsync(userId, request.Id);
            await Send.NoContentAsync(cancellationToken);
        }
    }
}
