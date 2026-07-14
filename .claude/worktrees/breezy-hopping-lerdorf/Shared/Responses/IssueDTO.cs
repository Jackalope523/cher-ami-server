using System;

namespace CherAmiAPI.Shared.Responses
{
    public record IssueDTO
    {
        public long Id { get; init; }
        public long CircleId { get; init; }
        public string Title { get; init; }
        public DateTimeOffset DraftingStart { get; init; }
        public DateTimeOffset DraftingEnd { get; init; }
    }
}
