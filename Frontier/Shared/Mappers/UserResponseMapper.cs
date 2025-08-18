using Core.Boundaries;
using FastEndpoints;
using Frontier.Contracts.Responses;

namespace CrazyLizard.Shared.SharedMappers
{
    public class UserResponseMapper : ResponseMapper<UserDTO, CoreUser>
    {
        public override UserDTO FromEntity(CoreUser user) => new UserDTO()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            FamilyName = user.LastName
        };
    }
}
