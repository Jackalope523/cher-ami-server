using CherAmiAPI.Services;
using FastEndpoints;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Auth.Email
{
    public class EmailAuthRequest
    {
        public string Email { get; set; }
    }

    public class EmailAuthRequestValidator : Validator<EmailAuthRequest>
    {
        public EmailAuthRequestValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email must be valid.")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));
        }
    }

    public class EmailAuthEndpoint(AuthService authService) : Endpoint<EmailAuthRequest>
    {
        public override void Configure()
        {
            Post("/auth/email");
            AllowAnonymous();
        }

        public override async Task HandleAsync(EmailAuthRequest request, CancellationToken cancellationToken)
        {
            await authService.SendEmailLoginCodeAsync(request.Email, cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
