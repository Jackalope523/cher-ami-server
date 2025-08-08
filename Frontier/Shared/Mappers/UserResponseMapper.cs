using FastEndpoints;
using Frontier.Contracts.Responses;

namespace LazyLizardBackend.Shared.SharedMappers
{
    public class UserResponseMapper : ResponseMapper<UserDTO, CoreUser>
    {
        public override UserDTO FromEntity(CoreUser user) => new UserDTO()
        {
            Id = user.Id,
            FirstName = user.GivenName,
            FamilyName = user.FamilyName
        };
    }
}
