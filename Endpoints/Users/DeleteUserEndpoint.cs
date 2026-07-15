using CherAmiAPI.Services;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Users
{
    public class DeleteUserEndpoint(UserService userService) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Delete("/user");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            await Send.NoContentAsync(cancellationToken);

            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await userService.DeleteUserAsync(userId, cancellationToken);
        }
    }
}
