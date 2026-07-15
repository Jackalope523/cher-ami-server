using FastEndpoints;
using FluentValidation;
using CherAmiAPI.Shared.Requests;

namespace CherAmiAPI.SharedContracts.SharedValidators
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
