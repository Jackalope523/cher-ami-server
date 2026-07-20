using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Exceptions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        public string PhotoUrl { get; set; }
        public string PhotoPath { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public string Caption { get; set; }
    }

    public class FeedPageResponse
    {
        public long? Id { get; set; }
        public string IssueTitle { get; set; }
        public DateTimeOffset? IssueDate { get; set; }
        public DateTimeOffset? IssueCloseDate { get; set; }
        public IssueStatus? Status { get; set; }
        public List<FeedPost> Posts { get; set; }
        public int? NextPage { get; set; }

    }

    public class FeedPostMapper(IConfiguration config) : ResponseMapper<FeedPost, Post>
    {
        public override FeedPost FromEntity(Post post)
        {
            return new()
            {
                Id = post.Id,
                AuthorId = post.AuthorId,
                // Legacy posts predate the PhotoDate column; fall back to the upload time.
                PhotoDate = post.PhotoDate == default ? post.PostedAt : post.PhotoDate,
                PhotoUrl = $"{config["APP_SERVICE_URI"]}/posts/{post.Id}/image?timestamp={post.PostedAt}",
                PhotoPath = $"/posts/{post.Id}/image",
                ImageWidth = post.ImageWidth != 0 ? post.ImageWidth : 259,
                ImageHeight = post.ImageHeight != 0 ? post.ImageHeight : 372,
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
                    IssueCloseDate = null,
                    Status = null,
                    NextPage = null,
                    Posts = [],
                };
            }

            return new()
            {
                Id = issue.Id,
                IssueTitle = issue.Title,
                IssueDate = issue.DraftingStart,
                IssueCloseDate = issue.DraftingEnd,
                Status = issue.Status,
                Posts = [.. issue.Posts
                    .OrderByDescending(x => x.PhotoDate == default ? x.PostedAt : x.PhotoDate)
                    .ThenByDescending(x => x.PostedAt)
                    .Select(feedPostMapper.FromEntity)],
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

            long circleId = await ctx.Users.Where(x => x.Id == userId).Select(x => x.CircleId).SingleAsync(cancellationToken: cancellationToken) ?? throw new NotFoundException("User does not have a circle.");

            List<long> blockedIds = await ctx.Blocks
                                    .Where(x => x.BlockerId == userId)
                                    .Select(x => x.BlockedId)
                                    .ToListAsync(cancellationToken: cancellationToken);

            List<long> blockedByIds = await ctx.Blocks
                                      .Where(x => x.BlockedId == userId)
                                      .Select(x => x.BlockerId)
                                      .ToListAsync(cancellationToken: cancellationToken);

            List<long> blacklist = [.. blockedIds, .. blockedByIds];

            Issue issue = await ctx.Issues
                            .Where(x => x.CircleId == circleId)
                            .Include(x => x.Posts.Where(x => !blacklist.Contains(x.AuthorId)))
                            .OrderByDescending(x => x.IssueNumber)
                            .Skip(request.PageParam)
                            .Take(1)
                            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

            FeedPageResponse response = Map.FromEntity(issue);

            if (issue != null) {
                int count = await ctx.Issues.CountAsync(cancellationToken: cancellationToken);
                response.NextPage = count > request.PageParam ? request.PageParam + 1 : null;
            }

            await Send.OkAsync(response, cancellationToken);
        }
    }
}