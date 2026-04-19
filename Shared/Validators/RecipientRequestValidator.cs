using CherAmiAPI.Shared.Requests;
using FastEndpoints;
using FluentValidation;

namespace CherAmiAPI.Shared.Validators
{
    public class RecipientRequestValidator : Validator<RecipientRequest>
    {
        public RecipientRequestValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(25).WithMessage("Title cannot exceed 25 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Title));

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(60).WithMessage("Name cannot exceed 60 characters.");

            RuleFor(x => x.AddressLine1)
                .NotEmpty().WithMessage("Address line 1 is required.")
                .MaximumLength(100).WithMessage("Address line 1 name cannot exceed 100 characters");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City name is required.")
                .MaximumLength(50).WithMessage("City name cannot exceed 50 characters");

            RuleFor(x => x.ProvinceOrState)
                .NotEmpty().WithMessage("Province or state name is required.")
                .MaximumLength(50).WithMessage("Province or state name cannot exceed 50 characters");

            RuleFor(x => x.PostalCode)
                .NotEmpty().WithMessage("Postal code is required.")
                .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters");

            RuleFor(x => x.Country)
                .NotEmpty().WithMessage("Country name is required.")
                .MaximumLength(56).WithMessage("Country name cannot exceed 56 characters");
        }
    }
}