using Core.Boundaries;
using System;

namespace CrazyLizard.Shared.Responses
{
    public record RecipientDTO
    {
        public long Id { get; init; }
        public long ManagerId { get; init; }
        public string FullName { get; init; }
        public Address Address { get; init; }
    }
}
