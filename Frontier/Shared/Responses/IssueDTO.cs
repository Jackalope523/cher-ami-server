using Core.Boundaries;
using System;

namespace LazyLizardBackend.Shared.Responses
{
    public record IssueDTO
    {
        public long Id { get; init; }
        public long CircleId { get; init; }
        public IssueType Type { get; init; }
        public string Title { get; init; }
        public DateTimeOffset StartDate { get; init; }
        public DateTimeOffset EndDate { get; init; }
    }
}
