using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Posts
{
    public class GetUploadIdRequest
    {
        public DateTime Time { get; set; }
    }

    public class IdResponse
    {
        public long Id { get; set; }
    }

    public class GetUploadIdEndpoint(ApplicationDbContext ctx) : Endpoint<GetUploadIdRequest, IdResponse>
    {
        public override void Configure()
        {
            Post("/issue/posts/get-upload-id");
        }

        public override async Task HandleAsync(GetUploadIdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            long? circleId = await ctx.Users
                .Where(x => x.Id == userId)
                .Select(x => x.CircleId)
                .SingleAsync(cancellationToken);

            if (circleId == null)
            {
                await Send.ForbiddenAsync(cancellationToken);
                return;
            }

            long currentIssueId = (await ctx.Issues
                .Where(x => x.CircleId == circleId)
                .ToListAsync(cancellationToken))
                .OrderByDescending(x => x.DraftingEnd)
                .Select(x => x.Id)
                .First();

            Post post = new()
            {
                AuthorId = userId,
                IssueId = currentIssueId,
                PostedAt = request.Time,
                SoftDeleted = true
            };

            ctx.Posts.Add(post);
            await ctx.SaveChangesAsync(cancellationToken);

            await Send.OkAsync(new IdResponse { Id = post.Id }, cancellationToken);
        }
    }
}