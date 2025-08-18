using FastEndpoints;
using Frontier.Endpoints.Account;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Core.Boundaries;

namespace CrazyLizard.Endpoints.Account
{
    public class CreateAccountRequest
    {
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string CircleCode { get; set; }
    }

    public class CreateAccountRequestValidator : Validator<CreateAccountRequest>
    {
        public CreateAccountRequestValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(20).WithMessage("Title cannot exceed 20 characters.");

            RuleFor(x => x.Title)
                .MaximumLength(25).WithMessage("Title cannot exceed 25 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Title));

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required.")
                .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email must be valid.")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.CircleCode)
                .MaximumLength(100).WithMessage("Circle code cannot exceed 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.CircleCode));
        }
    }

    public class CreateAccountEndpoint(UserManager<CoreUser> userManager, IAccountService accountService, ISMSService smsService) : Endpoint<CreateAccountRequest>
    {
        public override void Configure()
        {
            Post("/account/signup");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateAccountRequest request, CancellationToken cancellationToken)
        {
            var userExists = await accountService.GetUserExistsAsync(request.PhoneNumber);

            if (!userExists)
            {
                // Persist a new user
                await accountService.CreateUserAsync(request.PhoneNumber, request.Email,
                    request.Title, request.FirstName, request.LastName,
                    request.DateOfBirth.ToUniversalTime());

                // Send an SMS to new user with a generated change number token
                var user = await accountService.GetCoreUserAsync(request.PhoneNumber);
                var code = await userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
                await smsService.SendTextMessageAsync(user.PhoneNumber, $"Your Canary code is {code}");
            }
            else
            {
                // Account already exists
                var user = await accountService.GetCoreUserAsync(request.PhoneNumber);
                string code;

                // Login
                if (await userManager.IsPhoneNumberConfirmedAsync(user))
                {
                    code = await userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);
                }
                // Account is not activated, send an SMS with a generated change number token
                else
                {
                    code = await userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
                }

                await smsService.SendTextMessageAsync(user.PhoneNumber, $"Your Canary code is {code}");
            }

            await Send.CreatedAtAsync<GetAccountEndpoint>(cancellation: cancellationToken);
        }
    }
}