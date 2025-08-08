using FastEndpoints;
using FluentValidation;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace LazyLizardBackend.Endpoints.Circle
{
    public class CircleInviteRequest
    {
        public long CircleId { get; set; }

        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

    public class CircleInviteRequestValidator : AbstractValidator<CircleInviteRequest>
    {
        public CircleInviteRequestValidator()
        {
            RuleFor(x => x.CircleId)
                .GreaterThan(0).WithMessage("CircleId must be greater than zero.");

            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.PhoneNumber) || !string.IsNullOrWhiteSpace(x.Email))
                .WithMessage("Either PhoneNumber or Email must be provided.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email must be valid.")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));
        }
    }

    public class InviteUserEndpoint(ICircleService circles) : Endpoint<CircleInviteRequest>
    {
        public override void Configure()
        {
            Post("/circle/{circleId}/members");
        }

        public override async Task HandleAsync(CircleInviteRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await circles.SendInvitationAsync(userId, request.CircleId, request.PhoneNumber, request.Email);
            await Send.NoContentAsync(cancellationToken);
        }
    }
}