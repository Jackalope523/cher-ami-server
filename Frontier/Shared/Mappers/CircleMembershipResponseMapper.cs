using Core.Boundaries;
using FastEndpoints;
using LazyLizardBackend.Contracts.Responses;

namespace LazyLizardBackend.Shared.SharedMappers
{
    public class CircleMembershipResponseMapper : ResponseMapper<CircleMembershipDTO, CoreCircleMembership>
    {
        public override CircleMembershipDTO FromEntity(CoreCircleMembership membership) => new()
        {
            UserId = membership.UserId,
            DateJoined = membership.DateJoined,
            Type = membership.Type,
        };
    }
}
