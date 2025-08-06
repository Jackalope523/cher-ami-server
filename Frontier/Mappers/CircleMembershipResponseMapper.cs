using FastEndpoints;
using LazyLizardBackend.Contracts.Responses;

namespace Mappers
{
    public class CircleMembershipResponseMapper : ResponseMapper<CircleMembershipShard, CoreCircleMembership>
    {
        public override CircleMembershipShard FromEntity(CoreCircleMembership membership) => new()
        {
            UserId = membership.UserId,
            DateJoined = membership.DateJoined,
            Type = membership.Type,
        };
    }
}
