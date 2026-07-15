using CherAmiAPI.Entities;
using CherAmiAPI.Services;
using CherAmiAPI.Shared.Mappers;
using CherAmiAPI.Shared.Requests;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Users
{
    public class GetUserEndpoint(UserService userService) : Endpoint<IdRequest, UserDTO, UserResponseMapper>
    {
        public override void Configure()
        {
            Get("/users/{id}");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            User user = await userService.GetUserAsync(userId, request.Id, cancellationToken);

            await Send.OkAsync(Map.FromEntity(user), cancellationToken);
        }
    }
}
