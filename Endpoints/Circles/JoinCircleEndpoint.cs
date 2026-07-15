using CherAmiAPI.Services;
using FastEndpoints;
using FluentValidation;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Circles
{
    public class JoinCircleRequest
    {
        public string Code { get; set; }
    }

    public class JoinCircleRequestValidator : Validator<JoinCircleRequest>
    {
        public JoinCircleRequestValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Invite code is required.")
                .MaximumLength(100).WithMessage("Invite code cannot exceed 100 characters.");
        }
    }
    public class JoinCircleEndpoint(CircleService circleService) : Endpoint<JoinCircleRequest>
    {
        public override void Configure()
        {
            Post("/circles/join");
        }

        public override async Task HandleAsync(JoinCircleRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await circleService.JoinCircleAsync(userId, request.Code, cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
