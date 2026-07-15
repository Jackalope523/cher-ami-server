using CherAmiAPI.Services;
using FastEndpoints;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Auth.Email
{
    public class EmailVerifyRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }

    public class EmailVerifyRequestValidator : Validator<EmailVerifyRequest>
    {
        public EmailVerifyRequestValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email must be valid.")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required.")
                .MaximumLength(6).WithMessage("Code cannot exceed 6 characters.");
        }
    }

    public class EmailVerifyEndpoint(AuthService authService) : Endpoint<EmailVerifyRequest>
    {
        public override void Configure()
        {
            Post("/auth/email/verify");
            AllowAnonymous();
        }

        public override async Task HandleAsync(EmailVerifyRequest request, CancellationToken cancellationToken)
        {
            (string token, bool onboarded) = await authService.VerifyEmailLoginAsync(request.Email, request.Code, cancellationToken);

            await Send.OkAsync(new { Token = token, Onboarded = onboarded }, cancellationToken);
        }
    }
}
