using Microsoft.AspNetCore.Http;

namespace CherAmiAPI.Shared.Requests
{
    public record RecipientRequest
    {
        public IFormFile Avatar { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string ProvinceOrState { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }
}
