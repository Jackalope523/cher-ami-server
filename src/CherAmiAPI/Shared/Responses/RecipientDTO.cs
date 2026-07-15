using System;

namespace CherAmiAPI.Shared.Responses
{
    public record RecipientDTO
    {
        public long Id { get; init; }
        public long ManagerId { get; init; }
        public string AvatarUrl { get; init; }
        public string AvatarPath { get; init; }
        public DateTimeOffset? AvatarTimestamp { get; init; }
        public string Title { get; init; }
        public string Name { get; init; }
        public string AddressLine1 { get; init; }
        public string AddressLine2 { get; init; }
        public string City { get; init; }
        public string ProvinceOrState { get; init; }
        public string PostalCode { get; init; }
        public string Country { get; init; }
        public bool IsVeteran { get; init; }
    }
}
