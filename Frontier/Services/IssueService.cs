using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Core.Boundaries;
using LazyLizardBackend.Shared.Responses;

namespace LazyLizardBackend.Services
{
    public class IssueService(IIssueRepository issueRepository) : IIssueService
    {
        public Task<CorePost> AddPostAsync(long userId, long issueId, DateTimeOffset timestamp, string caption, MemoryStream image)
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

        public Task<CoreIssue> GetIssueAsync(long userId, long issueId)
        {
            throw new NotImplementedException();
        }

        public Task<List<CoreIssue>> GetIssuesForCircleAsync(long userId, long CircleId)
        {
            throw new NotImplementedException();
        }

        public Task<CorePost> GetPostAsync(long userId, long postId)
        {
            throw new NotImplementedException();
        }

        public Task<List<CorePost>> GetPostsForIssueAsync(long userId, long issueId)
        {
            throw new NotImplementedException();
        }
    }
}

