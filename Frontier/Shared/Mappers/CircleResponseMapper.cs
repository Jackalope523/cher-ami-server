using Core.Boundaries;
using FastEndpoints;
using CrazyLizard.Contracts.Responses;

namespace CrazyLizard.Shared.SharedMappers
{
    public class CircleResponseMapper : ResponseMapper<CircleDTO, CoreCircle>
    {
        public override CircleDTO FromEntity(CoreCircle circle) => new()
        {
            Id = circle.Id,
            InviteCode = circle.InviteCode,
            Title = circle.Title,
            DateCreated = circle.DateCreated,
            Schedule = circle.Schedule,
        };
    }
}
