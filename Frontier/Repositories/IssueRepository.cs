using Core.Boundaries;
using Microsoft.EntityFrameworkCore;
using Repository.Contexts;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class IssueRepository(LLContext ctx, IMediaRepository mediaRepository) : IIssueRepository
    {   
        public async Task<CoreIssue> GetIssueAsync(long issueId)
        {
            return await ctx.Issues.
                   Where(i => i.Id == issueId).
                   Select(i => new CoreIssue
                   (
                       i.Id, 
                       i.CircleId, 
                       i.Type, 
                       i.Title, 
                       i.DraftingStart,
                       i.DraftingEnd
                   )).
                   SingleAsync();
        }

        public async Task<List<CoreIssue>> GetIssuesForCircleAsync(long circleId)
        {
            return await ctx.Issues.
                   Where(i => i.CircleId == circleId).
                   Select(i => new CoreIssue
                   (
                       i.Id,
                       i.CircleId,
                       i.Type,
                       i.Title,
                       i.DraftingStart,
                       i.DraftingEnd
                   )).
                   ToListAsync();
        }

        public async Task<CorePost> AddPostAsync(long issueId, long userId, DateTimeOffset timestamp, string caption, MemoryStream image)
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
                };

                ctx.Posts.Add(postToAdd);
                await ctx.SaveChangesAsync();

                Snapshot snapshotToAdd = new()
                {
                    PostId = postToAdd.Id,
                    SequenceNumber = 0,
                };

                Caption captionToAdd = new()
                {
                    PostId = postToAdd.Id,
                    SequenceNumber = 1,
                    Text = caption
                };

                ctx.AddRange(snapshotToAdd, captionToAdd);
                await ctx.SaveChangesAsync();

                long circleId = await ctx.Issues.Where(x => x.Id == issueId).Select(x => x.CircleId).SingleAsync();
                
                await mediaRepository.UploadSnapshotAsync(circleId, issueId, postToAdd.Id, snapshotToAdd.Id, image);

                await transaction.CommitAsync();

                return new CorePost
                (
                    postToAdd.Id,
                    postToAdd.IssueId,
                    postToAdd.AuthorId,
                    postToAdd.PostedAt,
                    captionToAdd.Text
                );
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<CorePost>> GetPostsForIssueAsync(long issueId)
        {
            return await ctx.Issues.
            Where(i => i.Id == issueId).
            Join
            (
                ctx.Posts.Where(p => p.IssueId == issueId),
                i => i.Id,
                p => p.IssueId,
                (_, p) => new { p.Id, p.IssueId, p.AuthorId, p.PostedAt }
            ).
            Join
            (
                ctx.Captions,
                p => p.Id,
                c => c.PostId,
                (p, c) => new CorePost(p.Id, p.IssueId, p.AuthorId, p.PostedAt, c.Text)
            ).
            ToListAsync();
        }

        public async Task<CorePost> GetPostAsync(long postId)
        {
            return await ctx.Posts.
            Where(p => p.Id == postId).
            Join
            (
                ctx.Captions.Where(c => c.PostId == postId),
                p => p.Id,
                c => c.PostId,
                (p,c) => new CorePost(p.Id, p.IssueId, p.AuthorId, p.PostedAt, c.Text)
            ).
            SingleAsync();
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

            return await ctx.CircleMemberships.AnyAsync(x => x.CircleId == circleId && x.UserId == userId);
        }

        public async Task<bool> IsContributorToIssueOf(long userId, long postId)
        {
            long issueId = await ctx.Posts.Where(x => x.Id == postId).Select(x => x.IssueId).SingleAsync();

            long circleId = await ctx.Issues.Where(x => x.Id == issueId).Select(x => x.CircleId).SingleAsync();

            return await ctx.CircleMemberships.AnyAsync(x => x.CircleId == circleId && x.UserId == userId);
        }
    }
}
