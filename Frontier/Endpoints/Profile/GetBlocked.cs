using CrazyLizard.Entities;
using CrazyLizard.Interfaces.Service;
using FastEndpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Profile
{
    public record BlockedUserResponse
    {
        public long UserId { get; init; }
        public string FullName { get; init; }
    }

    public class BlockedResponseMapper : ResponseMapper<BlockedUserResponse, User>
    {
        public override BlockedUserResponse FromEntity(User user) => new()
        {
            UserId = user.Id,
            FullName = $"{user.Title} {user.FirstName} {user.LastName}",
        };
    }


    public class GetBlocked(IProfileService profileService) : EndpointWithoutRequest<List<BlockedUserResponse>, BlockedResponseMapper>
    {
        public override void Configure()
        {
            Get("/account/blocked");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<User> coreBlockedUsers = await profileService.GetBlockedUsersAsync(userId);

            await Send.OkAsync(coreBlockedUsers.Select(Map.FromEntity).ToList(), cancellationToken);
        }
    }
}
