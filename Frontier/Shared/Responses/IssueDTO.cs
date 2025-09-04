using Core.Boundaries;
using System;

namespace CrazyLizard.Shared.Responses
{
    public record IssueDTO
    {
        public long Id { get; init; }
        public long CircleId { get; init; }
        public IssueType Type { get; init; }
        public string Title { get; init; }
        public DateTimeOffset DraftingStart { get; init; }
        public DateTimeOffset DraftingEnd { get; init; }
    }
}
