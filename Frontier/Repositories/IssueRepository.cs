using Core.Boundaries;
using CrazyLizard.Contexts;
using Microsoft.EntityFrameworkCore;
using CrazyLizard.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CrazyLizard.Interfaces.Repository;

namespace CrazyLizard.Repositories
{
    public class IssueRepository(ApplicationDbContext ctx, IMediaRepository mediaRepository) : IIssueRepository
    {
        public async Task CreateIssue(long circleId)
        {
            DateTimeOffset now = DateTimeOffset.Now;
            string monthName = now.ToString("MMMM");

            int lastIssueNumber = await ctx.Issues.
                                Where(x => x.CircleId == circleId).
                                OrderByDescending(x => x.IssueNumber).
                                Select(x => x.IssueNumber).
                                FirstOrDefaultAsync();

            DateTimeOffset firstDay = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
            int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
            DateTimeOffset lastDay = new DateTimeOffset(now.Year, now.Month, daysInMonth, 23, 59, 59, now.Offset);


            Issue toAdd = new() 
            { 
                CircleId = circleId, 
                Title = $"{monthName} Issue", 
                IssueNumber = lastIssueNumber + 1, 
                DraftingStart = firstDay, 
                DraftingEnd = lastDay, 
                Status = IssueStatus.Drafting, 

            };

            ctx.Issues.Add(toAdd);
            await ctx.SaveChangesAsync();
        }

        public async Task<Issue> GetIssueAsync(long issueId)
        {
            return await ctx.Issues.Where(i => i.Id == issueId).SingleAsync();
        }

        public async Task<List<Issue>> GetIssuesForCircleAsync(long circleId)
        {
            return await ctx.Issues.Where(i => i.CircleId == circleId).ToListAsync();
        }

        public async Task<Post> AddPostAsync(long issueId, long userId, DateTimeOffset timestamp, string caption, MemoryStream image)
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                Post postToAdd = new()
                {
                    IssueId = issueId,
                    AuthorId = userId,
                    Layout = Post.LayoutType.Single,
                    PostedAt = timestamp,
                    Caption = caption,
                };

                ctx.Posts.Add(postToAdd);
                await ctx.SaveChangesAsync();
                
                await mediaRepository.UploadSnapshotAsync(snapshotToAdd.Id, image);

                await transaction.CommitAsync();

                return postToAdd;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Post>> GetPostsForIssueAsync(long issueId)
        {
            return await ctx.Issues.
            Where(i => i.Id == issueId).
            Join
            (
                ctx.Posts.Where(p => p.IssueId == issueId),
                i => i.Id,
                p => p.IssueId,
                (_, p) => p
            ).
            ToListAsync();
        }

        public async Task<Post> GetPostAsync(long postId)
        {
            return await ctx.Posts.Where(p => p.Id == postId).SingleAsync();
        }

        public async Task DeletePostAsync(long postId)
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                await ctx.Captions.Where(c => c.PostId == postId).ExecuteDeleteAsync();
                await ctx.Snapshots.Where(s => s.PostId == postId).ExecuteDeleteAsync();
                await ctx.Posts.Where(p => p.Id == postId).ExecuteDeleteAsync();

                await transaction.CommitAsync();

            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> IsOwner(long userId, long postId)
        {
            return await ctx.Posts.AnyAsync(x => x.Id == postId && x.AuthorId == userId);
        }

        public async Task<bool> IsDraft(long postId, DateTimeOffset now)
        {
            long issueId = await ctx.Posts.Where(x => x.Id == postId).Select(x => x.IssueId).SingleAsync();

            return await ctx.Issues.AnyAsync(x => x.Id == issueId && x.DraftingEnd > now);
        }

        public async Task<bool> IsContributor(long userId, long issueId)
        {
            long circleId = await ctx.Issues.Where(x => x.Id == issueId).Select(x => x.CircleId).SingleAsync();

            return await ctx.Users.AnyAsync(x => x.Id == userId && x.CircleId == circleId);
        }

        public async Task<bool> IsContributorToIssueOf(long userId, long postId)
        {
            long issueId = await ctx.Posts.Where(x => x.Id == postId).Select(x => x.IssueId).SingleAsync();

            long circleId = await ctx.Issues.Where(x => x.Id == issueId).Select(x => x.CircleId).SingleAsync();

            return await ctx.Users.AnyAsync(x => x.Id == userId && x.CircleId == circleId);
        }

        public async Task<bool> Exists(long issueId)
        {
            return await ctx.Issues.AnyAsync(x => x.Id == issueId);
        }

        public async Task<Issue> GetCurrentIssueAsync(long circleId)
        {
            // JACKALOPE: This runs in memory rn cause SQLite doesn't support the ordering of dates on db. Uncomment and use that.
            //return await ctx.Issues.
            //       Where(x => x.CircleId == circleId).
            //       OrderByDescending(x => x.DraftingStart).
            //       Select((x) => new CoreIssue(x.Id, x.CircleId, x.Type, x.Title, x.DraftingStart, x.DraftingEnd)).
            //       FirstAsync();

            List<Issue> issues =  await ctx.Issues.Where(x => x.CircleId == circleId).ToListAsync();

            return issues.OrderByDescending(x => x.DraftingStart).First();
        }
    }
}
