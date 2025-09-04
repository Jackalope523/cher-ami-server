using Core.Boundaries;
using FastEndpoints;
using FluentValidation;
using CrazyLizard.Contracts.Requests;
using CrazyLizard.Contracts.Responses;
using CrazyLizard.Shared.SharedMappers;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Endpoints.Circle;

namespace Frontier.Endpoints.Account
{
    public class CreateCircleRequest
    {
        public string Title { get; set; }
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

            RuleFor(x => x.Schedule)
                .IsInEnum().WithMessage("Schedule is required.");

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
                                        request.Schedule,
                                        stream
                                    );

            await Send.CreatedAtAsync<GetCircleEndpoint>(new IdRequest() { Id = coreCircle.Id }, Map.FromEntity(coreCircle), cancellation: cancellationToken);
        }
    }
}