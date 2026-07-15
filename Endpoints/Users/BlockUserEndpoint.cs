using CherAmiAPI.Services;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Users
{
    public class BlockUserEndpoint(UserService userService) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Post("/users/{id}/block");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            long targetId = Route<long>("id");

            await userService.BlockUserAsync(userId, targetId, cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
