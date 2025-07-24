using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace Core.Boundaries
{
	#region Schemas

    public enum IssueType
    { Digital, Newspaper, Magazine }

	public record CoreIssue(long Id, long CircleId, IssueType Type, string Title, DateTimeOffset StartDate, DateTimeOffset EndDate)
        : CoreOnlyData();

    public record CorePost(long Id, long IssueId, long UserId, DateTimeOffset Timestamp, string Caption) 
        : CoreOnlyData();

    public record IssueShard(long Id, long CircleId, IssueType Type, string Title, DateTimeOffset StartDate, DateTimeOffset EndDate);

    public record PostShard(long Id, long IssueId, long UserId, DateTimeOffset Timestamp, string Caption);

    public record GalleryShard(List<PostShard> Posts);

    #endregion

    #region Gates

    public interface IIssueDatabase
    {
        Task<CoreIssue> GetIssueAsync(long issueId);

        Task<List<CoreIssue>> GetIssuesForCircleAsync(long circleId);
        Task<List<CorePost>> GetPostsForIssueAsync(long issueId);

        Task<CorePost> GetPostAsync(long postId);
        Task<CorePost> AddPostAsync(long issueId, long userId, DateTimeOffset timestamp, string caption);
        Task DeletePostAsync(long postId);
    }

    public interface IIssueOperations
    {
        Task<IssueShard> GetIssueAsync(long userId, long issueId);

        Task<List<IssueShard>> GetIssuesForCircleAsync(long userId, long CircleId);
        Task<GalleryShard> GetPostsForIssueAsync(long userId, long issueId);

        Task<PostShard> GetPostAsync(long userId, long postId);
        Task<PostShard> AddPostAsync(long userId, long issueId,
            DateTimeOffset timestamp, string caption,
            MemoryStream image);
        Task EditPostAsync(long userId, long postId,
            DateTimeOffset? timestamp = null, string caption = null,
            MemoryStream image = null);
        Task DeletePostAsync(long userId, long postId);
    }

	#endregion
}

