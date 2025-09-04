using Core.Boundaries;
using CrazyLizard.Contracts.Requests;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Circle
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
                .NotEmpty().WithMessage("Code is required.")
                .MaximumLength(100).WithMessage("Code cannot exceed 100 characters.");
        }
    }

    public class JoinCircleEndpoint(ICircleService circles) : Endpoint<JoinCircleRequest>
    {
        public override void Configure()
        {
            Post("/circles/join");
        }

        public override async Task HandleAsync(JoinCircleRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await circles.AddMemberAsync(userId, request.Code);
            await Send.NoContentAsync(cancellationToken);
        }
    }
}
