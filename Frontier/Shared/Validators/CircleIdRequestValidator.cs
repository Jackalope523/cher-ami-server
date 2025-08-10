using FastEndpoints;
using FluentValidation;
using Frontier.Contracts.Requests;
using Frontier.Endpoints.Account;

namespace LazyLizardBackend.SharedContracts.SharedValidators
{
    public class CircleIdRequestValidator : Validator<CircleIdRequest>
    {
        public CircleIdRequestValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}
