using CherAmiAPI.Services;
using CherAmiAPI.Shared.Requests;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Users
{
    public class UnblockUserEndpoint(UserService userService) : Endpoint<IdRequest>
    {
        public override void Configure()
        {
            Delete("/users/{id}/block");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await userService.UnblockUserAsync(userId, request.Id, cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
