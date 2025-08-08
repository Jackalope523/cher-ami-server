using System;

namespace LazyLizardBackend.Contracts.Responses
{
    public enum CircleMembershipType
    { 
        Regular, 
        Owner 
    }

    public record CircleMembershipDTO
    {
        public long UserId { get; init; }
        public DateTimeOffset DateJoined { get; init; }
        public CircleMembershipType Type { get; init; }
    }
}
