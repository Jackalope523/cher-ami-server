using CherAmiAPI.Entities;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CherAmiAPI.Shared.SharedMappers
{
    public class UserItemMapper(IConfiguration config) : ResponseMapper<UserItem, User>
    {
        public override UserItem FromEntity(User user) => new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarUrl = user.AvatarPath == null ? null : $"{config["APP_SERVICE_URI"]}/users/{user.Id}/avatar?timestamp={user.AvatarTimestamp}",
            AvatarPath = user.AvatarPath == null ? null : $"/users/{user.Id}/avatar",
            AvatarTimestamp = user.AvatarTimestamp
        };
    }
}
