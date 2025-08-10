using FastEndpoints;
using FluentValidation;
using LazyLizardBackend.Shared.Requests;

namespace LazyLizardBackend.Shared.Validators
{
    public class IssueIdRequestValidator : Validator<IssueIdRequest>
    {
        public IssueIdRequestValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}
