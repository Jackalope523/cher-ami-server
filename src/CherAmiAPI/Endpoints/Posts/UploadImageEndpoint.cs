using CherAmiAPI.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Posts
{
    public class UploadImageRequest
    {
        public string UploadId { get; set; }
        public IFormFile Image { get; set; }
    }

    public class UploadImageEndpoint(PostService postService) : Endpoint<UploadImageRequest>
    {
        public override void Configure()
        {
            Post("/issue/posts/upload-image");
            AllowFileUploads();
        }

        public override async Task HandleAsync(UploadImageRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await postService.UploadImageAsync(userId, request.UploadId, request.Image, cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
