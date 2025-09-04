using Core.Boundaries;
using System;

namespace CrazyLizard.Contracts.Responses
{
    public record CircleDTO
    {
        public long Id { get; init; }
        public string InviteCode { get; init; }
        public string Title { get; init; }
        public DateTimeOffset DateCreated { get; init; }
        public IssueSchedule Schedule { get; init; }
    }
}
