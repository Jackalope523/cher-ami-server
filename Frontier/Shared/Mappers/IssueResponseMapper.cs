using Core.Boundaries;
using FastEndpoints;
using CrazyLizard.Shared.Responses;

namespace CrazyLizard.Shared.Mappers
{
    public class IssueResponseMapper : ResponseMapper<IssueDTO, CoreIssue>
    {
        public override IssueDTO FromEntity(CoreIssue issue) => new()
        {
            Id = issue.Id,
            CircleId = issue.CircleId,
            Type = issue.Type,
            Title = issue.Title,
            DraftingStart = issue.DraftingStart,
            DraftingEnd = issue.DraftingEnd,
        };
    }
}
