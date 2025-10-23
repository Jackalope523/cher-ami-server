using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Entities;
using CrazyLizard.Shared.Responses;
using CrazyLizard.Shared.Mappers;

namespace CrazyLizard.Endpoints.Users
{
    public class GetAccountEndpoint(UserManager<User> userManager) : EndpointWithoutRequest<UserDTO, UserResponseMapper>
    {
        public override void Configure()
        {
            Get("/account");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            User user = await userManager.GetUserAsync(HttpContext.User);
            await SendMapped(user, 200, cancellationToken);
        }
    }
}
