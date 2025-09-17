using Core.Boundaries;
using FastEndpoints;
using CrazyLizard.Contracts.Responses;

namespace CrazyLizard.Shared.SharedMappers
{
    public class CircleMembershipResponseMapper : ResponseMapper<CircleMembershipDTO, CoreCircleMembership>
    {
        public override CircleMembershipDTO FromEntity(CoreCircleMembership membership) => new()
        {
            UserId = membership.UserId,
            DateJoined = membership.DateJoined,
        };
    }
}
