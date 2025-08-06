using System;

namespace Frontier.Contracts.Responses
{
    public record RecipientShard
    {
        public long Id { get; init; }
        public long ManagerId { get; init; }
        public string FullName { get; init; }
        public DateTimeOffset DateOfBirth { get; init; }
        public Address Address { get; init; }
    }
}
