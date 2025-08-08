using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;
namespace Frontier.Endpoints.Account
{
    public class VerifyEmailRequest
    {
        public string Token { get; set; }
        public string Email { get; set; }
    }

    public class VerifyEmailRequestValidator : Validator<VerifyEmailRequest>
    {
        public VerifyEmailRequestValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Token is required.");

            RuleFor(x => x.Email)
              .NotEmpty().WithMessage("Email is required.")
              .EmailAddress().WithMessage("Email must be valid.")
              .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");
        }
    }

    public class VerifyEmailEndpoint(UserManager<CoreUser> userManager) : Endpoint<VerifyEmailRequest>
    {
        public override void Configure()
        {
            Get("/account/email");
            AllowAnonymous();
        }

        public override async Task HandleAsync(VerifyEmailRequest request, CancellationToken cancellationToken)
        {

            CoreUser user = await userManager.FindByEmailAsync(request.Email);

            if (user != null)
            {
                await userManager.ConfirmEmailAsync(user, request.Token);
                await Send.NoContentAsync(cancellationToken);
            }
            else
            {
                await Send.NotFoundAsync(cancellationToken);
            }
        }
    }
}