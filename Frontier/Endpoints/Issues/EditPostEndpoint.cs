using FastEndpoints;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using System.IO;
using Core.Boundaries;

namespace CrazyLizard.Endpoints.Issue
{
    public class EditPostRequest
    {
        public long PostId { get; set; }
        public DateTime? Time { get; set; }
        public string Caption { get; set; }
        public IFormFile Image { get; set; }
    }

    public class EditPostRequestValidator : Validator<EditPostRequest>
    {
        public EditPostRequestValidator()
        {
            RuleFor(x => x.PostId)
               .GreaterThan(0).WithMessage("PostId must be greater than 0.");

            RuleFor(x => x.Time)
                .NotEmpty().WithMessage("Time is required.")
                .When(x => x.Time != null);

            RuleFor(x => x.Image)
                .Must(x => x.Length > 0).WithMessage("Uploaded image can not be empty.")
                .When(x => x.Image != null);

            RuleFor(x => x.Caption)
                .MaximumLength(200).WithMessage("Caption cannot exceed 200 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Caption));

            RuleFor(x => x)
                .Must(x => x.Time.HasValue || !string.IsNullOrWhiteSpace(x.Caption) || x.Image != null)
                .WithMessage("You must provide at least one of: Time, Caption, or Image.");
        }
    }

    public class EditPostEndpoint(IIssueService issues) : Endpoint<EditPostRequest>
    {
        public override void Configure()
        {
            Put("/issues/posts/{postId}");
            AllowFileUploads();
        }

        public override async Task HandleAsync(EditPostRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            using MemoryStream stream = new();
            if (request.Image != null && request.Image.Length > 0)
            {
                await request.Image.CopyToAsync(stream);
            }

            await issues.EditPostAsync(userId, request.PostId, request.Time, request.Caption, stream);
            await Send.NoContentAsync(cancellationToken);
        }
    }
}