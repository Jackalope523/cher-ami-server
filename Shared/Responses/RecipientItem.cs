using System;

namespace CherAmiAPI.Shared.Responses
{
    public record RecipientItem
    {
        public long Id { get; init; }
        public long ManagerId { get; init; }
        public string Name { get; init; }
        public string AvatarUrl { get; init; }
        public string AvatarPath { get; init; }
        public DateTimeOffset? AvatarTimestamp { get; init; }
        public bool IsVeteran { get; init; }
    }
}
