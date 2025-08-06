using FastEndpoints;
using Frontier.Contracts.Responses;

namespace Mappers
{
    public class UserResponseMapper : ResponseMapper<UserShard, CoreUser>
    {
        public override UserShard FromEntity(CoreUser user) => new UserShard()
        {
            Id = user.Id,
            FirstName = user.GivenName,
            FamilyName = user.FamilyName
        };
    }
}
