using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Entities.Reports;
using CherAmiAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Repositories
{
    public class PostRepository(ApplicationDbContext ctx) : IPostRepository
    {
        public async Task<Post> GetPostAsync(long postId, CancellationToken cancellationToken = default)
        {
            return await ctx.Posts.FindAsync([postId], cancellationToken: cancellationToken);
        }

        public async Task<Post> GetPostByUploadIdAsync(string uploadId, CancellationToken cancellationToken = default)
        {
            return await ctx.Posts
                .IgnoreQueryFilters()
                .Include(x => x.Issue)
                .ThenInclude(x => x.Circle)
                .SingleOrDefaultAsync(x => x.UploadId == uploadId, cancellationToken);
        }

        public async Task AddPostAsync(Post post, CancellationToken cancellationToken = default)
        {
            ctx.Posts.Add(post);
            await ctx.SaveChangesAsync(cancellationToken);
        }

        public async Task SavePostAsync(Post post, CancellationToken cancellationToken = default)
        {
            await ctx.SaveChangesAsync(cancellationToken);
        }

        public async Task RemovePostAsync(Post post, CancellationToken cancellationToken = default)
        {
            ctx.Posts.Remove(post);
            await ctx.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> IsAuthorAsync(long postId, long userId, CancellationToken cancellationToken = default)
        {
            return await ctx.Posts.AnyAsync(x => x.Id == postId && x.AuthorId == userId, cancellationToken: cancellationToken);
        }

        public async Task CreatePostReportAsync(long postId, long filingUserId, CancellationToken cancellationToken = default)
        {
            PostReport report = new()
            {
                FilingUserId = filingUserId,
                FilingDate = DateTimeOffset.UtcNow,
                PostId = postId,
                Type = PostReportType.Other,
            };

            ctx.PostReports.Add(report);
            await ctx.SaveChangesAsync(cancellationToken);
        }

        public async Task<long> GetCurrentIssueIdAsync(long circleId, CancellationToken cancellationToken = default)
        {
            // Ordered client-side because SQLite can not reliably order DateTimeOffset columns server-side.
            return (await ctx.Issues
                .Where(x => x.CircleId == circleId)
                .ToListAsync(cancellationToken: cancellationToken))
                .OrderByDescending(x => x.DraftingEnd)
                .Select(x => x.Id)
                .First();
        }

        public async Task<long> GetFirstIssueIdOfCircleAsync(long circleId, CancellationToken cancellationToken = default)
        {
            return await ctx.Issues
                .Where(x => x.CircleId == circleId)
                .Select(x => x.Id)
                .FirstAsync(cancellationToken: cancellationToken);
        }

        public async Task<Issue> GetFeedPageAsync(long circleId, int page, List<long> excludedAuthorIds, CancellationToken cancellationToken = default)
        {
            return await ctx.Issues
                .Where(x => x.CircleId == circleId)
                .Include(x => x.Posts.Where(x => !excludedAuthorIds.Contains(x.AuthorId)))
                .OrderByDescending(x => x.IssueNumber)
                .Skip(page)
                .Take(1)
                .SingleOrDefaultAsync(cancellationToken: cancellationToken);
        }

        public async Task<int> CountIssuesOfCircleAsync(long circleId, CancellationToken cancellationToken = default)
        {
            return await ctx.Issues
                .Where(x => x.CircleId == circleId)
                .CountAsync(cancellationToken: cancellationToken);
        }

        public async Task<int> GetLatestIssuePostCountAsync(long circleId, CancellationToken cancellationToken = default)
        {
            return await ctx.Issues
                .Where(x => x.CircleId == circleId)
                .OrderByDescending(x => x.IssueNumber)
                .Select(x => x.Posts.Count)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }

        public async Task<long> GetCircleIdOfPostAsync(long postId, CancellationToken cancellationToken = default)
        {
            return await ctx.Posts
                .Where(x => x.Id == postId)
                .Select(x => x.Issue.CircleId)
                .SingleAsync(cancellationToken: cancellationToken);
        }

        public async Task<string> GetLowResolutionImagePathAsync(long postId, CancellationToken cancellationToken = default)
        {
            return await ctx.Posts
                .Where(x => x.Id == postId)
                .Select(x => x.LowResolutionImagePath)
                .SingleAsync(cancellationToken: cancellationToken);
        }

        public async Task<List<string>> GetImagePathsByAuthorAsync(long authorId, CancellationToken cancellationToken = default)
        {
            return await ctx.Posts
                .Where(x => x.AuthorId == authorId)
                .Select(x => x.LowResolutionImagePath)
                .ToListAsync(cancellationToken: cancellationToken);
        }
    }
}
