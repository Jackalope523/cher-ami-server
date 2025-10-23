using CrazyLizard.Entities;
using CrazyLizard.Shared.Responses;
using CrazyLizard.Shared.SharedMappers;
using FastEndpoints;
using System.Linq;

namespace CrazyLizard.Shared.Mappers
{
    public class UserResponseMapper(RecipientItemMapper mapper) : ResponseMapper<UserDTO, User>
    {
        public override UserDTO FromEntity(User user) => new()
        { 
            Id = user.Id,
            AvatarPath = $"/users/{user.Id}/avatar",
            AvatarTimestamp = user.AvatarTimestamp,
            Title = user.Title,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DateOfBirth = user.DateOfBirth,
            JoinDate = user.JoinDate,
            Recipients = user.Recipients.Select(mapper.FromEntity).ToList(),
        };
    }
}
