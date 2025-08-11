using FastEndpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace LazyLizardBackend.Endpoints.Profile
{
    public record BlockedUserResponse
    {
        public long UserId { get; init; }
        public string FullName { get; init; }
        public DateTimeOffset DateBlocked { get; init; }
    }

    public class BlockedResponseMapper : ResponseMapper<BlockedUserResponse, CoreBlockedUser>
    {
        public override BlockedUserResponse FromEntity(CoreBlockedUser user) => new()
        {
            UserId = user.UserId,
            FullName = user.FullName,
            DateBlocked = user.DateBlocked,
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

            List<CoreBlockedUser> coreBlockedUsers = await profileService.GetBlockedUsersAsync(userId);

            await Send.OkAsync(coreBlockedUsers.Select(Map.FromEntity).ToList(), cancellationToken);
        }
    }
}
