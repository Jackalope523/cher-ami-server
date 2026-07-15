using Microsoft.AspNetCore.Http;

namespace CherAmiAPI.Shared.Requests
{
    public class ImageRequest
    {
        public IFormFile Image { get; set; }
    }
}
