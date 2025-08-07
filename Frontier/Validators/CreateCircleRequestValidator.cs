using FastEndpoints;
using FluentValidation;
using Frontier.Contracts.Requests;

namespace LazyLizardBackend.Validators
{
    public class CreateCircleRequestValidator : Validator<CreateCircleRequest>
    {
        public CreateCircleRequestValidator() 
        {
            RuleFor(x => x.Title).
            NotEmpty().WithMessage("Title is required.").
            MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");
        }
    }
}
