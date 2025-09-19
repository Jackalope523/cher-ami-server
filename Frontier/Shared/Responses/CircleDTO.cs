using CrazyLizard.Entities;
using System;

namespace CrazyLizard.Shared.Responses
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
