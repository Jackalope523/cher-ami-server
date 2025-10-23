using CrazyLizard.Boundaries.Service;
using CrazyLizard.Entities;
using CrazyLizard.Interfaces.Service;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Users
{
    public class LoginRequest
    {
        public string PhoneNumber { get; set; }
    }

    public class LoginRequestValidator : Validator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("PhoneNumber is required.");
        }
    }


    public class LoginEndpoint(UserManager<User> userManager, IAccountService accountService) : Endpoint<LoginRequest>
    {
        public override void Configure()
        {
            Post("/account/login");
            AllowAnonymous();
        }

        public override async Task HandleAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            var user = await accountService.GetCoreUserAsync(request.PhoneNumber);

            if (user == null)
            {
                await Send.UnauthorizedAsync(cancellationToken);
                return;
            }

            string code;

            if (await userManager.IsPhoneNumberConfirmedAsync(user))
            {
                code = await userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);
            }
            else
            {
                code = await userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
            }


            //await smsService.SendTextMessageAsync(user.PhoneNumber, $"Your Lazy Lizard code is {code}");

            await Send.NoContentAsync(cancellationToken);
        }
    }
}