using CrazyLizard.Shared.Requests;
using FastEndpoints;
using FluentValidation;

namespace CrazyLizard.Shared.Validators
{
    public class RecipientRequestValidator : Validator<RecipientRequest>
    {
        public RecipientRequestValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(25).WithMessage("Title cannot exceed 25 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Title));

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

            RuleFor(x => x.UnitNumber)
                .NotEmpty().WithMessage("Unit number is required.")
                .MaximumLength(15).WithMessage("Unit number cannot exceed 15 characters");

            RuleFor(x => x.Street)
                .NotEmpty().WithMessage("Street name is required.")
                .MaximumLength(150).WithMessage("Street name cannot exceed 150 characters");

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