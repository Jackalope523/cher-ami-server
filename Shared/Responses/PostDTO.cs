using System;

namespace CrazyLizard.Shared.Responses
{
    public record PostDTO
    {
        public long Id { get; init; }
        public long IssueId { get; init; }
        public long AuthorId { get; init; }
        public DateTimeOffset PostedAt { get; init; }
        public string Caption { get; init; }
    }
}
