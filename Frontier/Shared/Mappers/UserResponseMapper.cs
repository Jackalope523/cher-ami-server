using CrazyLizard.Entities;
using FastEndpoints;
using Frontier.Contracts.Responses;

namespace CrazyLizard.Shared.SharedMappers
{
    public class UserResponseMapper : ResponseMapper<UserDTO, User>
    {
        public override UserDTO FromEntity(User user) => new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarPath = $"/media/avatars/{user.Id}",
        };
    }
}
