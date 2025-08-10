using FastEndpoints;
using FluentValidation;
using Frontier.Contracts.Requests;
using LazyLizardBackend.Contracts.Responses;
using LazyLizardBackend.Shared.SharedMappers;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class CreateCircleRequest
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public CirclePlan Plan { get; set; }

        [Required]
        public IssueSchedule Schedule { get; set; }

        public IFormFile Image { get; set; }
    }

    public class CreateCircleRequestValidator : Validator<CreateCircleRequest>
    {
        public CreateCircleRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

            RuleFor(x => x.Plan)
                .NotEmpty().WithMessage("Plan is required.");

            RuleFor(x => x.Schedule)
                .NotEmpty().WithMessage("Plan is required.");

            RuleFor(x => x.Image)
                    .Must(file => file.Length > 0).WithMessage("Image cannot be empty.")
                    .When(x => x.Image != null);
        }
    }

    public class CreateCircleEndpoint(ICircleService circles) : Endpoint<CreateCircleRequest, CircleDTO, CircleResponseMapper>
    {
        public override void Configure()
        {
            Post("/circle");
            AllowFileUploads();
        }

        public override async Task HandleAsync(CreateCircleRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            using MemoryStream stream = new();
            await request.Image.CopyToAsync(stream, cancellationToken);

            CoreCircle coreCircle = await circles.CreateCircleAsync(
                                        userId,
                                        request.Title,
                                        request.Plan,
                                        request.Schedule,
                                        stream
                                    );

            await Send.CreatedAtAsync<GetCircleEndpoint>(new CircleIdRequest() { Id = coreCircle.Id }, Map.FromEntity(coreCircle), cancellation: cancellationToken);
        }
    }
}