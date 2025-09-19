using Core.Boundaries;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Entities;

namespace CrazyLizard.Endpoints.Circle
{
    public class CircleEditRequest
    {
        public string Title { get; set; }

        public IssueSchedule Schedule { get; set; }

        public IFormFile Image { get; set; }
    }

    public class CircleEditRequestValidator : Validator<CircleEditRequest>
    {
        public CircleEditRequestValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Title));

            RuleFor(x => x.Image)
                .Must(x => x.Length > 0).WithMessage("Image cannot be empty.")
                .When(x => x.Image != null);
        }
    }

    public class EditCircleEndpoint(ICircleService circles) : Endpoint<CircleEditRequest>
    {
        public override void Configure()
        {
            Post("/circle/{circleId}/edit");
            AllowFileUploads();
            AllowFormData();
        }

        public override async Task HandleAsync(CircleEditRequest request, CancellationToken cancellationToken)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var circleId = Route<long>("circleId");

            using var stream = new MemoryStream();
            if (request.Image is { Length: > 0 })
                await request.Image.CopyToAsync(stream, cancellationToken);

            await circles.EditCircleAsync(
                userId,
                circleId,
                title: request.Title,
                schedule: request.Schedule,
                header: stream
            );

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
