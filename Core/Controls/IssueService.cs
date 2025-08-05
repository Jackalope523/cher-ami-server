using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Core.Boundaries;

namespace Core.Controls
{
    public class IssueService : AbstractService, IIssueOperations
	{
		public IssueService(CoreTerminal terminal) : base(terminal) { }

        public Task<PostShard> AddPostAsync(long userId, long issueId, DateTimeOffset timestamp, string caption, MemoryStream image)
        {
            throw new NotImplementedException();
        }

        public Task DeletePostAsync(long userId, long postId)
        {
            throw new NotImplementedException();
        }

        public Task EditPostAsync(long userId, long postId, DateTimeOffset? timestamp = null, string caption = null, MemoryStream image = null)
        {
            throw new NotImplementedException();
        }

        public Task<IssueShard> GetIssueAsync(long userId, long issueId)
        {
            throw new NotImplementedException();
        }

        public Task<List<IssueShard>> GetIssuesForCircleAsync(long userId, long CircleId)
        {
            throw new NotImplementedException();
        }

        public Task<PostShard> GetPostAsync(long userId, long postId)
        {
            throw new NotImplementedException();
        }

        public Task<GalleryShard> GetPostsForIssueAsync(long userId, long issueId)
        {
            throw new NotImplementedException();
        }
    }
}

