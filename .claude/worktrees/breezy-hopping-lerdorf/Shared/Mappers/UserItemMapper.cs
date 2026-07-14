using CherAmiAPI.Entities;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;

namespace CherAmiAPI.Shared.SharedMappers
{
    public class UserItemMapper : ResponseMapper<UserItem, User>
    {
        public override UserItem FromEntity(User user) => new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarPath = user.AvatarPath == null ? null : $"/users/{user.Id}/avatar",
            AvatarTimestamp = user.AvatarTimestamp,
        };
    }
}
