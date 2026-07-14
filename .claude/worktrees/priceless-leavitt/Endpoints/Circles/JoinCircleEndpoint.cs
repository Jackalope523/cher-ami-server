using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Exceptions;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
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
    public class JoinCircleEndpoint(ApplicationDbContext ctx) : Endpoint<JoinCircleRequest>
    {
        public override void Configure()
        {
            Post("/circles/join");
        }

        public override async Task HandleAsync(JoinCircleRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            User user = await ctx.Users.Where(x => x.Id == userId).SingleAsync(cancellationToken: cancellationToken);

            if (user.CircleId != null)
                throw new NoPermissionException($"User {userId} already has a circle.");

            long circleId = await ctx.Circles.Where(x => x.CircleCode == request.Code).Select(x => x.Id).SingleOrDefaultAsync(cancellationToken: cancellationToken);

            if (circleId == 0)
                throw new NotFoundException($"Invalid invite code.");

            user.CircleId = circleId;
            user.CircleJoinDate = DateTimeOffset.UtcNow;
            await ctx.SaveChangesAsync(cancellationToken);
            
            await Send.NoContentAsync(cancellationToken);
        }
    }
}
