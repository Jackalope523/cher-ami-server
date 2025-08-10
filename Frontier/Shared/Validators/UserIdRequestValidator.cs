using FastEndpoints;
using FluentValidation;
using Frontier.Contracts.Requests;
using Frontier.Endpoints.Account;

namespace LazyLizardBackend.SharedContracts.SharedValidators
{
    public class UserIdRequestValidator : Validator<UserIdRequest>
    {
        public UserIdRequestValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id must be greater than 0.")
                .When(x => x.Id != -2 || x.Id != -7 || x.Id != -8);
        }
    }
}