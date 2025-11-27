using CherAmiAPI.Entities;
using CherAmiAPI.Shared.Responses;
using CherAmiAPI.Shared.SharedMappers;
using FastEndpoints;
using System.Linq;

namespace CherAmiAPI.Shared.Mappers
{
    public class UserResponseMapper(RecipientItemMapper mapper) : ResponseMapper<UserDTO, User>
    {
        public override UserDTO FromEntity(User user) => new()
        { 
            Id = user.Id,
            AvatarPath = user.AvatarPath == null ? null : $"/users/{user.Id}/avatar",
            AvatarTimestamp = user.AvatarTimestamp,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DateOfBirth = user.DateOfBirth,
            JoinDate = user.JoinDate,
            Recipients = [.. user.Recipients.Select(mapper.FromEntity)],
        };
    }
}
