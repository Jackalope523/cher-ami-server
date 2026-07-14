using CherAmiAPI.Endpoints.Circles;
using CherAmiAPI.Shared.Requests;
using FastEndpoints;
using FluentValidation;

namespace CherAmiAPI.SharedContracts.SharedValidators
{
    public class ImageRequestValidator : Validator<ImageRequest>
    {
        public ImageRequestValidator()
        {
            RuleFor(x => x.Image)
                .NotNull().WithMessage("Image is required.")
                .Must(file => file.Length > 0).WithMessage("Uploaded image can not be empty.");
        }
    }
}
