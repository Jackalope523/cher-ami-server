using CherAmiAPI.Services;
using FastEndpoints;
using FluentValidation;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Posts
{
    public class UploadImageDetailsRequest
    {
        public string UploadId { get; set; }
        public string Caption { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class UploadImageDetailsValidator : Validator<UploadImageDetailsRequest>
    {
        public UploadImageDetailsValidator()
        {
            RuleFor(x => x.Caption)
                .MaximumLength(200).WithMessage("Caption cannot exceed 200 characters.");
        }
    }

    public class UploadImageDetailsEndpoint(PostService postService) : Endpoint<UploadImageDetailsRequest>
    {
        public override void Configure()
        {
            Post("/issue/posts/upload-details");
        }

        public override async Task HandleAsync(UploadImageDetailsRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            bool processed = await postService.ProcessImageDetailsAsync(userId, request.UploadId, request.Caption, request.X, request.Y, request.Width, request.Height, cancellationToken);

            if (!processed)
                return;

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
