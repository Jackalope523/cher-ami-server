using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace Core.Boundaries
{
	#region Schemas

    public enum IssueType
    { Magazine }

	public record CoreIssue(long Id, long CircleId, IssueType Type, string Title, DateTimeOffset StartDate, DateTimeOffset EndDate)
        : CoreOnlyData();

    public record CorePost(long Id, long IssueId, long UserId, DateTimeOffset Timestamp, string Caption) 
        : CoreOnlyData();

    #endregion

    #region Gates

    public interface IIssueRepository
    {
        Task CreateIssue(long circleId);
        Task<bool> Exists(long issueId);
        Task<bool> IsContributor(long userId, long issueId);
        Task<CoreIssue> GetIssueAsync(long issueId);

        Task<List<CoreIssue>> GetIssuesForCircleAsync(long circleId);
        Task<List<CorePost>> GetPostsForIssueAsync(long issueId);

        Task<bool> IsOwner(long userId, long postId);
        Task<bool> IsDraft(long postId, DateTimeOffset now);
        Task<bool> IsContributorToIssueOf(long userId, long postId);
        Task<CorePost> GetPostAsync(long postId);
        Task<CorePost> AddPostAsync(long issueId, long userId, DateTimeOffset timestamp, string caption, MemoryStream image);
        Task DeletePostAsync(long postId);
    }

    public interface IIssueService
    {
        Task<CoreIssue> GetIssueAsync(long userId, long issueId);

        Task<List<CoreIssue>> GetIssuesForCircleAsync(long userId, long circleId);
        Task<List<CorePost>> GetPostsForIssueAsync(long userId, long issueId);

        Task<CorePost> GetPostAsync(long userId, long postId);
        Task<CorePost> AddPostAsync(long userId, long issueId,
            DateTimeOffset timestamp, string caption,
            MemoryStream image);
        Task EditPostAsync(long userId, long postId,
            DateTimeOffset? timestamp = null, string caption = null,
            MemoryStream image = null);
        Task DeletePostAsync(long userId, long postId);
    }

	#endregion
}

