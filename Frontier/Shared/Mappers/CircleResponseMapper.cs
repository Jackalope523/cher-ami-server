using FastEndpoints;
using CrazyLizard.Entities;
using CrazyLizard.Shared.Responses;

namespace CrazyLizard.Shared.SharedMappers
{
    public class CircleResponseMapper : ResponseMapper<CircleDTO, Circle>
    {
        public override CircleDTO FromEntity(Circle circle) => new()
        {
            Id = circle.Id,
            InviteCode = circle.CircleCode,
            Title = circle.Title,
            DateCreated = circle.TimeOfCreation,
            Schedule = circle.IssueSchedule,
        };
    }
}
