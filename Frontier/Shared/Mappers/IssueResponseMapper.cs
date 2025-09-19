using FastEndpoints;
using CrazyLizard.Shared.Responses;
using CrazyLizard.Entities;

namespace CrazyLizard.Shared.Mappers
{
    public class IssueResponseMapper : ResponseMapper<IssueDTO, Issue>
    {
        public override IssueDTO FromEntity(Issue issue) => new()
        {
            Id = issue.Id,
            CircleId = issue.CircleId,
            Title = issue.Title,
            DraftingStart = issue.DraftingStart,
            DraftingEnd = issue.DraftingEnd,
        };
    }
}
