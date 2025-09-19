using CrazyLizard.Boundaries.Service;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class ImageRequest
    {
        [Required]
        public IFormFile Image { get; set; }
    }

    public class ImageRequestValidator : Validator<ImageRequest>
    {
        public ImageRequestValidator()
        {
            RuleFor(x => x.Image)
                .NotNull().WithMessage("Image is required.")
                .Must(file => file.Length > 0).WithMessage("Image cannot be empty.");
        }
    }

    public class ModifyAvatarEndpoint(IAccountService accountService) : Endpoint<ImageRequest>
    {
        public override void Configure()
        {
            Post("/account/avatar");
            AllowFileUploads();
        }

        public override async Task HandleAsync(ImageRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            using var stream = new MemoryStream();
            await request.Image.CopyToAsync(stream);

            await accountService.EditAvatarAsync(userId, stream);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}