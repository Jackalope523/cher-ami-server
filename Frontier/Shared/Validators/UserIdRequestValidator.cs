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
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("UserId must be greater than 0.")
                .When(x => x.UserId != -2 || x.UserId != -7 || x.UserId != -8);
        }
    }
}