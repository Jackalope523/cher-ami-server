using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace Core.Boundaries
{
	#region Schemas

    public enum IssueType
    { Digital, Newspaper, Magazine }

	public record CoreIssue(long Id, long GroupId, IssueType Type, string Title, DateTimeOffset StartDate, DateTimeOffset EndDate)
        : CoreOnlyData();

    public record IssueShard(long Id, long GroupId, IssueType Type, string Title, DateTimeOffset StartDate, DateTimeOffset EndDate);

    public record PostShard(long Id, long IssueId, long UserId, DateTimeOffset Timestamp, string Caption);

    public record GalleryShard(List<PostShard> Posts);

    #endregion

    #region Gates

    public interface IIssueDatabase
    {
        Task<List<CoreIssue>> GetIssuesForGroupAsync(long groupId);
        Task<List<PostShard>> GetPostsForIssueAsync(long issueId);

        Task<PostShard> GetPostAsync(long postId);
        Task<PostShard> AddPostAsync(long issueId, long userId, DateTimeOffset timestamp, string caption);

        Task SoftDeleteAsync(long postId);
        Task HardDeleteAsync(long postId);
    }

    public interface IIssueOperations
    {
        Task<List<IssueShard>> GetIssuesForGroupAsync(long userId, long groupId);
        Task<GalleryShard> GetPostsForIssueAsync(long userId, long issueId);

        Task<PostShard> GetPostAsync(long userId, long postId);
        Task<PostShard> AddPostAsync(long userId, long issueId,
            DateTimeOffset timestamp, string caption,
            MemoryStream image);
        Task EditPostAsync(long userId, long postId,
            DateTimeOffset? timestamp = null, string caption = null);
        Task DeletePostAsync(long userId, long postId);
    }

	#endregion
}

