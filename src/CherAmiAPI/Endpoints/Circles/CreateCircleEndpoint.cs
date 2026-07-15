using CherAmiAPI.Entities;
using CherAmiAPI.Services;
using CherAmiAPI.Shared.Requests;
using CherAmiAPI.Shared.Responses;
using CherAmiAPI.Shared.SharedMappers;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Circles
{
    public class CreateCircleRequest
    {
        public string Title { get; set; }
        public IFormFile Image { get; set; }
    }

    public class CreateCircleRequestValidator : Validator<CreateCircleRequest>
    {
        public CreateCircleRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

            RuleFor(x => x.Image)
                    .Must(file => file.Length > 0).WithMessage("Image cannot be empty.");
        }
    }

    public class CreateCircleEndpoint(CircleService circleService) : Endpoint<CreateCircleRequest, CircleDTO, CircleResponseMapper>
    {
        public override void Configure()
        {
            Post("/circle");
            AllowFileUploads();
        }

        public override async Task HandleAsync(CreateCircleRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Circle created = await circleService.CreateCircleAsync(userId, request.Title, request.Image, cancellationToken);

            await Send.CreatedAtAsync<GetCircleEndpoint>(new IdRequest() { Id = created.Id }, Map.FromEntity(created), cancellation: cancellationToken);
        }
    }
}
