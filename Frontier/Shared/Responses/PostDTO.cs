using System;

namespace LazyLizardBackend.Contracts.Responses
{
    public record PostDTO
    {
        public long Id { get; init; }
        public long IssueId { get; init; }
        public long UserId { get; init; }
        public DateTimeOffset Timestamp { get; init; }
        public string Caption { get; init; }
    }
}
