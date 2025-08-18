using Core.Boundaries;
using FastEndpoints;
using FluentValidation;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class AccountEditRequest
    {
        public string Email { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }

    public class AccountEditRequestValidator : Validator<AccountEditRequest>
    {
        public AccountEditRequestValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(25).WithMessage("Title cannot exceed 25 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Title));

            RuleFor(x => x.FirstName)
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

            RuleFor(x => x.LastName)
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.LastName));

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past.")
                .When(x => x.DateOfBirth.HasValue);

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email must be valid.")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));
        }
    }

    public class ModifyAccount(IAccountService accountService) : Endpoint<AccountEditRequest>
    {
        public override void Configure()
        {
            Put("/account");
            AllowFormData();
        }

        public override async Task HandleAsync(AccountEditRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await accountService.EditUserAsync(userId,
                  email: request.Email,
                  title: request.Title, givenName: request.FirstName, familyName: request.LastName,
                  dateOfBirth: request.DateOfBirth);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}