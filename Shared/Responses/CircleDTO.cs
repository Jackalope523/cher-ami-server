using CherAmiAPI.Entities;
using System;
using System.Collections.Generic;

namespace CherAmiAPI.Shared.Responses
{
    public record CircleDTO
    {
        public long Id { get; init; }
        public string HeaderUrl { get; init; }
        public string HeaderPath { get; init; }
        public DateTimeOffset? HeaderTimestamp { get; init; }
        public string Title { get; init; }
        public string InviteCode { get; init; }
        public DateTimeOffset DateCreated { get; init; }
        public IssueSchedule Schedule { get; init; }
        public List<UserItem> Contributors { get; init; }
        public List<RecipientItem> Recipients { get; init; }
    }
}
