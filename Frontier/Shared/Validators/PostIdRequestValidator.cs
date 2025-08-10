using FastEndpoints;
using FluentValidation;
using Frontier.Contracts.Requests;

namespace LazyLizardBackend.SharedContracts.SharedValidators
{
    public class PostIdRequestValidator : Validator<CircleIdRequest>
    {
        public PostIdRequestValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}
