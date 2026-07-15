using CherAmiAPI.Entities;
using CherAmiAPI.Services;
using FastEndpoints;
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
                PhotoDate = post.PostedAt,
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
                    NextPage = null,
                    Posts = [],
                };
            }

            return new()
            {
                Id = issue.Id,
                IssueTitle = issue.Title,
                IssueDate = issue.DraftingStart,
                Posts = [.. issue.Posts.OrderByDescending(x => x.PostedAt).Select(feedPostMapper.FromEntity)],
            };
        }
    }

    public class GetIssueEndpoint(PostService postService) : Endpoint<FeedPageRequest, FeedPageResponse, FeedPageResponseMapper>
    {
        public override void Configure()
        {
            Get("/circle/issues/{pageParam}");
        }

        public override async Task HandleAsync(FeedPageRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            (Issue issue, int? nextPage) = await postService.GetFeedPageAsync(userId, request.PageParam, cancellationToken);

            FeedPageResponse response = Map.FromEntity(issue);

            if (issue != null)
            {
                response.NextPage = nextPage;
            }

            await Send.OkAsync(response, cancellationToken);
        }
    }
}
