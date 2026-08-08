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

    /// <summary>
    /// The columns the feed actually needs — the Post entity also carries image
    /// paths and upload bookkeeping the response never exposes.
    /// </summary>
    public class FeedPostData
    {
        public long Id { get; set; }
        public long AuthorId { get; set; }
        public DateTimeOffset PhotoDate { get; set; }
        public DateTimeOffset PostedAt { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public string Caption { get; set; }
    }

    public class FeedIssueData
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public DateTimeOffset DraftingStart { get; set; }
        public DateTimeOffset DraftingEnd { get; set; }
        public IssueStatus Status { get; set; }
        public List<FeedPostData> Posts { get; set; }
    }

    public class FeedPostMapper(IConfiguration config) : ResponseMapper<FeedPost, FeedPostData>
    {
        public override FeedPost FromEntity(FeedPostData post)
        {
            return new()
            {
                Id = post.Id,
                AuthorId = post.AuthorId,
                PhotoDate = post.PhotoDate == default ? post.PostedAt : post.PhotoDate,
                PhotoUrl = $"{config["APP_SERVICE_URI"]}/posts/{post.Id}/image?timestamp={post.PostedAt}",
                PhotoPath = $"/posts/{post.Id}/image",
                ImageWidth = post.ImageWidth != 0 ? post.ImageWidth : 259,
                ImageHeight = post.ImageHeight != 0 ? post.ImageHeight : 372,
                Caption = post.Caption,
            };
        }
    }

    public class FeedPageResponseMapper(FeedPostMapper feedPostMapper) : ResponseMapper<FeedPageResponse, FeedIssueData>
    {
        public override FeedPageResponse FromEntity(FeedIssueData issue)
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
                // Legacy posts predate the PhotoDate column; fall back to upload time.
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

            FeedIssueData issue = await ctx.Issues
                            .Where(x => x.CircleId == circleId)
                            .OrderByDescending(x => x.IssueNumber)
                            .Skip(request.PageParam)
                            .Take(1)
                            .Select(x => new FeedIssueData
                            {
                                Id = x.Id,
                                Title = x.Title,
                                DraftingStart = x.DraftingStart,
                                DraftingEnd = x.DraftingEnd,
                                Status = x.Status,
                                Posts = x.Posts
                                    .Where(p => !blacklist.Contains(p.AuthorId))
                                    .Select(p => new FeedPostData
                                    {
                                        Id = p.Id,
                                        AuthorId = p.AuthorId,
                                        PhotoDate = p.PhotoDate,
                                        PostedAt = p.PostedAt,
                                        ImageWidth = p.ImageWidth,
                                        ImageHeight = p.ImageHeight,
                                        Caption = p.Caption,
                                    })
                                    .ToList(),
                            })
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