using CherAmiAPI.Services;
using CherAmiAPI.Shared.Requests;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Users
{
    public class UpdateUserAvatarEndpoint(UserService userService) : Endpoint<ImageRequest>
    {
        public override void Configure()
        {
            Post("/user/avatar");
            AllowFileUploads();
        }

        public override async Task HandleAsync(ImageRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await userService.UpdateAvatarAsync(userId, request.Image, cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
