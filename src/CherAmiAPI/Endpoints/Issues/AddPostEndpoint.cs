using CherAmiAPI.Services;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Issues
{
    public class CreatePostRequest
    {
        public DateTime Time { get; set; }
        public string Caption { get; set; }
        public IFormFile Image { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
    }

    public class CreatePostRequestValidator : Validator<CreatePostRequest>
    {
        public CreatePostRequestValidator()
        {
            RuleFor(x => x.Time)
                .NotEmpty().WithMessage("Time is required.");

            RuleFor(x => x.Image)
                .NotNull().WithMessage("Image is required.")
                .Must(file => file.Length > 0).WithMessage("Uploaded image can not be empty.");

            RuleFor(x => x.Caption)
                .MaximumLength(200).WithMessage("Caption cannot exceed 200 characters.");
        }
    }

    public class AddPostEndpoint(PostService postService) : Endpoint<CreatePostRequest>
    {
        public override void Configure()
        {
            Post("/issue/posts");
            AllowFileUploads();
        }

        public override async Task HandleAsync(CreatePostRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await postService.AddPostAsync(userId, request.Time, request.Caption, request.Image, request.ImageWidth, request.ImageHeight, cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
