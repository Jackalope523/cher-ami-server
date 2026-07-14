using System;

namespace CherAmiAPI.Shared.Responses
{
    public record RecipientItem
    {
        public long Id { get; init; }
        public long ManagerId { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string AvatarPath { get; init; }
        public DateTimeOffset AvatarTimestamp { get; init; }
    }
}
