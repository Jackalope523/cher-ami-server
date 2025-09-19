using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using CrazyLizard.Entities;

namespace CrazyLizard.Interfaces.Repository
{
    public interface IIssueRepository
    {
        Task CreateIssue(long circleId);
        Task<bool> Exists(long issueId);
        Task<bool> IsContributor(long userId, long issueId);
        Task<Issue> GetIssueAsync(long issueId);
        Task<Issue> GetCurrentIssueAsync(long circleId);

        Task<List<Issue>> GetIssuesForCircleAsync(long circleId);
        Task<List<Post>> GetPostsForIssueAsync(long issueId);

        Task<bool> IsOwner(long userId, long postId);
        Task<bool> IsDraft(long postId, DateTimeOffset now);
        Task<bool> IsContributorToIssueOf(long userId, long postId);
        Task<Post> GetPostAsync(long postId);
        Task<Post> AddPostAsync(long issueId, long userId, DateTimeOffset timestamp, string caption, MemoryStream image);
        Task DeletePostAsync(long postId);
    }
}

