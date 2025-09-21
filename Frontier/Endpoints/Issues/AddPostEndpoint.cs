using FastEndpoints;
using CrazyLizard.Shared.Mappers;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using System.IO;
using CrazyLizard.Contracts.Requests;
using CrazyLizard.Shared.Responses;
using CrazyLizard.Interfaces.Service;
using CrazyLizard.Entities;

namespace CrazyLizard.Endpoints.Issues
{
    public class CreatePostRequest
    {
        public long IssueId { get; set; }
        public DateTime Time { get; set; }
        public string Caption { get; set; }
        public IFormFile Image { get; set; }
    }

    public class CreatePostRequestValidator : Validator<CreatePostRequest>
    {
        public CreatePostRequestValidator()
        {
            RuleFor(x => x.IssueId)
               .GreaterThan(0).WithMessage("IssueId must be greater than 0.");

            RuleFor(x => x.Time)
                .NotEmpty().WithMessage("Time is required.");

            RuleFor(x => x.Image)
                .NotNull().WithMessage("Image is required.")
                .Must(file => file.Length > 0).WithMessage("Uploaded image can not be empty.");

            RuleFor(x => x.Caption)
                .MaximumLength(200).WithMessage("Caption cannot exceed 200 characters.");
        }
    }

    public class AddPostEndpoint(IIssueService issues) : Endpoint<CreatePostRequest, PostDTO, PostResponseMapper>
    {
        public override void Configure()
        {
            Post("/issues/{issueId}/posts");
            AllowFileUploads();
        }

        public override async Task HandleAsync(CreatePostRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            using var stream = new MemoryStream();
            await request.Image.CopyToAsync(stream, cancellationToken);

            Post corePost = await issues.AddPostAsync(userId, request.IssueId, request.Time, request.Caption, stream);

            await Send.CreatedAtAsync<GetPostEndpoint>
                (
                    new IdRequest() { Id = corePost.Id },
                    Map.FromEntity(corePost),
                    cancellation: cancellationToken
                );
        }
    }
}