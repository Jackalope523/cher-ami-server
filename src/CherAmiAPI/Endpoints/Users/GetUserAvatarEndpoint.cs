using CherAmiAPI.Services;
using CherAmiAPI.Shared.Requests;
using FastEndpoints;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Users
{
    public class GetUserAvatarEndpoint(UserService userService) : Endpoint<IdRequest, FileStreamResult>
    {
        public override void Configure()
        {
            Get("/users/{id}/avatar");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            MemoryStream image = await userService.GetAvatarAsync(userId, request.Id, cancellationToken);

            await Send.StreamAsync(image, cancellation: cancellationToken);
        }
    }
}
