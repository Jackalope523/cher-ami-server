using System;

namespace CrazyLizard.Shared.Responses
{
    public record RecipientDTO
    {
        public long Id { get; init; }
        public long ManagerId { get; init; }
        public string AvatarPath { get; init; }
        public DateTimeOffset AvatarTimestamp { get; init; }
        public string Title { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string Street { get; init; }
        public string City { get; init; }
        public string ProvinceOrState { get; init; }
        public string PostalCode { get; init; }
        public string Country { get; init; }
        public string UnitNumber { get; init; }

    }
}
