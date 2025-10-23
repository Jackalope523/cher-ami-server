using CrazyLizard.Entities;
using CrazyLizard.Shared.Responses;
using FastEndpoints;

namespace CrazyLizard.Shared.SharedMappers
{
    public class UserItemMapper : ResponseMapper<UserItem, User>
    {
        public override UserItem FromEntity(User user) => new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarPath = $"/users/{user.Id}/avatar",
            AvatarTimestamp = user.AvatarTimestamp,
        };
    }
}
