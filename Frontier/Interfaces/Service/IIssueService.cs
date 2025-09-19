using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using CrazyLizard.Entities;

namespace CrazyLizard.Interfaces.Service
{
    public interface IIssueService
    {
        Task<Issue> GetIssueAsync(long userId, long issueId);
        Task<Issue> GetCurrentIssueAsync(long userId);

        Task<List<Issue>> GetIssuesForCircleAsync(long userId, long circleId);
        Task<List<Post>> GetPostsForIssueAsync(long userId, long issueId);

        Task<Post> GetPostAsync(long userId, long postId);
        Task<Post> AddPostAsync(long userId, long issueId,
            DateTimeOffset timestamp, string caption,
            MemoryStream image);
        Task EditPostAsync(long userId, long postId,
            DateTimeOffset? timestamp = null, string caption = null,
            MemoryStream image = null);
        Task DeletePostAsync(long userId, long postId);
    }
}

