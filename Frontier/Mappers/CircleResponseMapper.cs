using FastEndpoints;
using LazyLizardBackend.Contracts.Responses;

namespace Mappers
{
    public class CircleResponseMapper : ResponseMapper<CircleDTO, CoreCircle>
    {
        public override CircleDTO FromEntity(CoreCircle circle) => new()
        {
            Id = circle.Id,
            InviteCode = circle.InviteCode,
            Title = circle.Title,
            DateCreated = circle.DateCreated,
            Plan = circle.Plan,
            Schedule = circle.Schedule,
        };
    }
}
