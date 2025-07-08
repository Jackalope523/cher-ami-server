using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class IssueRepository : Repository, IIssueDatabase
    {   
        internal IssueRepository(Func<CanaryContext> contextFactory) : base(contextFactory)
        {
        }

        public Task<CoreIssue> GetIssueAsync(long issueId)
        {
            throw new NotImplementedException();
        }

        public Task<List<CoreIssue>> GetIssuesForCircleAsync(long circleId)
        {
            throw new NotImplementedException();
        }

        public Task<PostShard> AddPostAsync(long issueId, long userId, DateTimeOffset timestamp, string caption)
        {
            throw new NotImplementedException();
        }

        public Task<List<PostShard>> GetPostsForIssueAsync(long issueId)
        {
            throw new NotImplementedException();
        }

        public Task<PostShard> GetPostAsync(long postId)
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
