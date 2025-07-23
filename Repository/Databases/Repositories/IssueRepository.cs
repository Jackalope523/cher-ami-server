using Microsoft.EntityFrameworkCore;
using Repository.Databases.Contexts;
using Repository.Databases.Entities;

namespace Repository.Databases.Stores
{
    public class IssueRepository : Repository, IIssueDatabase
    {   
        internal IssueRepository(Func<CanaryContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task<CoreIssue> GetIssueAsync(long issueId)
        {
            await using CanaryContext ctx = initContext();

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
            await using CanaryContext ctx = initContext();

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

        public async Task<CorePost> AddPostAsync(long issueId, long userId, DateTimeOffset timestamp, string caption)
        {
            await using CanaryContext ctx = initContext();
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                Post toAdd = new()
                {
                    IssueId = issueId,
                    AuthorId = userId,
                    Layout = Post.LayoutType.Single,
                    PostedAt = timestamp,
                };

                ctx.Posts.Add(toAdd);
                await ctx.SaveChangesAsync();

                Caption captionToAdd = new()
                {
                    PostId = toAdd.Id,
                    Text = caption
                };

                await transaction.CommitAsync();

                return new CorePost
                (
                    toAdd.Id,
                    toAdd.IssueId,
                    toAdd.AuthorId,
                    toAdd.PostedAt,
                    captionToAdd.Text
                );
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public Task<List<CorePost>> GetPostsForIssueAsync(long issueId)
        {
            throw new NotImplementedException();
        }

        public Task<CorePost> GetPostAsync(long postId)
        {
            throw new NotImplementedException();
        }

        public Task SoftDeleteAsync(long postId)
        {
            throw new NotImplementedException();
        }

        public Task HardDeleteAsync(long postId)
        {
            throw new NotImplementedException();
        }
    }
}
