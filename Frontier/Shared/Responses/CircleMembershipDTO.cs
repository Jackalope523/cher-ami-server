using System;

namespace CrazyLizard.Contracts.Responses
{
    public record CircleMembershipDTO
    {
        public long UserId { get; init; }
        public DateTimeOffset DateJoined { get; init; }
    }
}
