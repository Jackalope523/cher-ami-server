using System;

namespace CrazyLizard.Shared.Responses
{
    public record CircleMembershipDTO
    {
        public long UserId { get; init; }
        public DateTimeOffset DateJoined { get; init; }
    }
}
