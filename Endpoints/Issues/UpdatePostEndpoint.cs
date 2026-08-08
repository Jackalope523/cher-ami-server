using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Exceptions;
using CherAmiAPI.Interfaces;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
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

    public class UpdatePostEndpoint(ApplicationDbContext ctx, IPhotoDateService photoDateService) : Endpoint<UpdatePostRequest>
    {
        public override void Configure()
        {
            Put("/posts/{id}");
        }

        public override async Task HandleAsync(UpdatePostRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var post = await ctx.Posts
                .Where(x => x.Id == request.Id)
                .Select(x => new
                {
                    x.AuthorId,
                    x.PhotoDate,
                    IssueStatus = x.Issue.Status,
                    x.Issue.DraftingStart,
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (post == null)
            {
                await Send.NotFoundAsync(cancellationToken);
                return;
            }

            if (post.AuthorId != userId)
                throw new NoAccessException($"User {userId} is not the author of post {request.Id}.");

            // Once an issue leaves drafting the magazine is locked in; published
            // posts can only be deleted, not edited.
            if (post.IssueStatus != IssueStatus.Drafting)
            {
                await Send.ForbiddenAsync(cancellationToken);
                return;
            }

            DateTimeOffset photoDate = photoDateService.Normalize(request.PhotoDate ?? post.PhotoDate, post.DraftingStart);

            await ctx.Posts
                .Where(x => x.Id == request.Id)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(p => p.Caption, request.Caption)
                    .SetProperty(p => p.PhotoDate, photoDate), cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
