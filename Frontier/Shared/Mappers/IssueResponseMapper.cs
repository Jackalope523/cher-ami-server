using Core.Boundaries;
using FastEndpoints;
using LazyLizardBackend.Shared.Responses;

namespace LazyLizardBackend.Shared.Mappers
{
    public class IssueResponseMapper : ResponseMapper<IssueDTO, CoreIssue>
    {
        public override IssueDTO FromEntity(CoreIssue issue) => new()
        {
            Id = issue.Id,
            CircleId = issue.CircleId,
            Type = issue.Type,
            Title = issue.Title,
            StartDate = issue.StartDate,
            EndDate = issue.EndDate,
        };
    }
}
