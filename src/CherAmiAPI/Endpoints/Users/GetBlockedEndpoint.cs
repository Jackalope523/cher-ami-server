using CherAmiAPI.Contexts;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Services;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Users
{
    public class GetBlockedEndpoint(UserService userService) : EndpointWithoutRequest<List<UserItem>>
    {
        public override void Configure()
        {
            Get("/users/blocked");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<UserItem> blockedUsers = await userService.GetBlockedUsersAsync(userId, cancellationToken);

            await Send.OkAsync(blockedUsers, cancellationToken);
        }
    }
}