using CherAmiAPI.Services;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Circles
{
    public class UpdateCircleRequest
    {
        public string Title { get; set; }
        public IFormFile Header { get; set; }
    }

    public class UpdateCircleRequestValidator : Validator<UpdateCircleRequest>
    {
        public UpdateCircleRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

            RuleFor(x => x.Header)
                .Must(x => x.ContentType == "image/jpeg" || x.ContentType == "image/jpg").WithMessage("Image must be a jpeg.")
                .Must(x => x.Length > 0).WithMessage("Image cannot be empty.")
                .Must(x => x.Length <= 5 * 1024 * 1024).WithMessage("Image cannot exceed 5MB.")
                .When(x => x.Header != null);
        }
    }

    public class UpdateCircleEndpoint(CircleService circleService) : Endpoint<UpdateCircleRequest>
    {
        public override void Configure()
        {
            Put("/circle");
            AllowFileUploads();
        }

        public override async Task HandleAsync(UpdateCircleRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await circleService.UpdateCircleAsync(userId, request.Title, request.Header, cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
