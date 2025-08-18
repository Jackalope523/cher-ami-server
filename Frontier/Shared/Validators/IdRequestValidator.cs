using FastEndpoints;
using FluentValidation;
using CrazyLizard.Contracts.Requests;

namespace CrazyLizard.SharedContracts.SharedValidators
{
    public class IdRequestValidator : Validator<IdRequest>
    {
        public IdRequestValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}
