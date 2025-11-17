using Azure.Security.KeyVault.Certificates;
using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Shared.Responses;
using CherAmiAPI.Shared.SharedMappers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Circles
{
    public class FeedPageRequest
    {
        public int PageParam { get; set; }
    }
    public class FeedPost
    {
        public long Id { get; set; }
        public long AuthorId { get; set; }
        public DateTimeOffset PhotoDate { get; set; }
        public string PhotoPath { get; set; }
        public string Caption { get; set; }
    }

    public class FeedPageResponse
    {
        public long? Id { get; set; }
        public string IssueTitle { get; set; }
        public DateTimeOffset? IssueDate { get; set; }
        public List<FeedPost> Posts { get; set; }
        public int? NextPage { get; set; }

    }

    public class FeedPostMapper : ResponseMapper<FeedPost, Post>
    {
        public override FeedPost FromEntity(Post post)
        {
            return new()
            {
                Id = post.Id,
                AuthorId = post.AuthorId,
                PhotoDate = post.PostedAt,
                PhotoPath = $"/posts/{post.Id}/image",
                Caption = post.Caption,
            };
        }
    }

    public class FeedPageResponseMapper(FeedPostMapper feedPostMapper) : ResponseMapper<FeedPageResponse, Issue>
    {
        public override FeedPageResponse FromEntity(Issue issue)
        {
            if (issue == null)
            {
                return new()
                {
                    Id = null,
                    IssueTitle = null,
                    IssueDate = null,
                    NextPage = null,
                    Posts = [],
                };
            }

            return new()
            {
                Id = issue.Id,
                IssueTitle = issue.Title,
                IssueDate = issue.DraftingEnd,
                Posts = issue.Posts.OrderByDescending(x => x.PostedAt).Select(feedPostMapper.FromEntity).ToList(),
            };
        }
    }

    public class GetIssueEndpoint(ApplicationDbContext ctx) : Endpoint<FeedPageRequest, FeedPageResponse, FeedPageResponseMapper>
    {
        public override void Configure()
        {
            Get("/circle/issues/{pageParam}");
        }

        public override async Task HandleAsync(FeedPageRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            long? circleId = await ctx.Users.Where(x => x.Id == userId).Select(x => x.CircleId).SingleAsync(cancellationToken: cancellationToken);

            // JACKALOPE: SQL Date bullshit. This is prod version.
            //Issue issue = await ctx.Issues
            //                .Where(x => x.CircleId == circleId)
            //                .Include(x => x.Posts)
            //                .ThenInclude(x => x.Author)
            //                .OrderByDescending(x => x.DraftingEnd)
            //                .Skip(request.PageParam)
            //                .Take(1)
            //                .SingleOrDefaultAsync(cancellationToken: cancellationToken);

            Issue issue = (await ctx.Issues
                            .Where(x => x.CircleId == circleId)
                            .Include(x => x.Posts)
                            .ThenInclude(x => x.Author)
                            .ToListAsync())
                            .OrderByDescending(x => x.DraftingEnd)
                            .Skip(request.PageParam)
                            .Take(1)
                            .SingleOrDefault();

            FeedPageResponse response = Map.FromEntity(issue);

            if (issue != null) {
                int count = await ctx.Issues.CountAsync();
                response.NextPage = count > request.PageParam ? request.PageParam + 1 : null;
            }

            await Send.OkAsync(response, cancellationToken);
        }
    }
}