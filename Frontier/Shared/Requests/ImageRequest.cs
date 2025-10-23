using Microsoft.AspNetCore.Http;

namespace CrazyLizard.Shared.Requests
{
    public class ImageRequest
    {
        public IFormFile Image { get; set; }
    }
}
