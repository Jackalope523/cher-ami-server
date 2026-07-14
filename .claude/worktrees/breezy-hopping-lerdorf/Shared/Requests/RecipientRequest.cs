using Microsoft.AspNetCore.Http;

namespace CherAmiAPI.Shared.Requests
{
    public record RecipientRequest
    {
        public IFormFile Avatar { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UnitNumber { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string ProvinceOrState { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }
}
