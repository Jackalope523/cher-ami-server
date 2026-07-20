using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Exceptions;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Issues
{
    public class UpdatePostRequest
    {
        public long Id { get; set; }
        public string Caption { get; set; }
        public DateTimeOffset? PhotoDate { get; set; }
    }

    public class UpdatePostRequestValidator : Validator<UpdatePostRequest>
    {
        public UpdatePostRequestValidator()
        {
            RuleFor(x => x.Caption)
                .MaximumLength(200).WithMessage("Caption cannot exceed 200 characters.");
        }
    }

    public class UpdatePostEndpoint(ApplicationDbContext ctx) : Endpoint<UpdatePostRequest>
    {
        public override void Configure()
        {
            Put("/posts/{id}");
        }

        public override async Task HandleAsync(UpdatePostRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Post post = await ctx.Posts
                .Include(x => x.Issue)
                .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (post == null)
            {
                await Send.NotFoundAsync(cancellationToken);
                return;
            }

            if (post.AuthorId != userId)
                throw new NoAccessException($"User {userId} is not the author of post {post.Id}.");

            // Once an issue leaves drafting the magazine is locked in; published
            // posts can only be deleted, not edited.
            if (post.Issue.Status != IssueStatus.Drafting)
            {
                await Send.ForbiddenAsync(cancellationToken);
                return;
            }

            post.Caption = request.Caption;
            post.PhotoDate = Shared.PhotoDates.Normalize(request.PhotoDate ?? post.PhotoDate, post.Issue.DraftingStart);

            await ctx.SaveChangesAsync(cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
