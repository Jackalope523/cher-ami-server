using Core.Boundaries;
using System;

namespace LazyLizardBackend.Contracts.Responses
{
    public enum CirclePlan
    { 
        None, 
        Digital, 
        Newspaper_30, 
        Newspaper_60, 
        Magazine_30 
    }

    public record CircleDTO
    {
        public long Id { get; init; }
        public string InviteCode { get; init; }
        public string Title { get; init; }
        public DateTimeOffset DateCreated { get; init; }
        public CirclePlan Plan { get; init; }
        public IssueSchedule Schedule { get; init; }
    }
}
