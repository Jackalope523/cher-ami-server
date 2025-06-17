using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace Core.Boundaries
{
	#region Schemas

    public enum SegmentType
    { Digital, Newspaper, Magazine }

	public record CoreSegment(long Id, long GroupId, SegmentType Type, string Title, DateTimeOffset TimeOpened, DateTimeOffset TimeClosed)
        : CoreOnlyData();

    public record SegmentShard(long Id, long GroupId, SegmentType Type, string Title, DateTimeOffset TimeOpened, DateTimeOffset TimeClosed);

    public record PostShard(long Id, long SegmentId, long UserId, DateTimeOffset TimePosted);

    public record GalleryShard(List<PostShard> Posts);

    #endregion

    #region Gates

    public interface ISegmentDatabase
    {
        Task<List<CoreSegment>> GetSegmentsForGroupAsync(long groupId);
        Task<List<PostShard>> GetPostsForSegmentAsync(long segmentId);

        Task<PostShard> GetPostAsync(long postId);
        Task<PostShard> AddPostAsync(long segmentId, long userId, DateTimeOffset timePosted);

        Task SoftDeleteAsync(long postId);
        Task HardDeleteAsync(long postId);
    }

    public interface ISegmentOperations
    {
        Task<List<SegmentShard>> GetSegmentsForGroupAsync(long userId, long groupId);
        Task<GalleryShard> GetGalleryAsync(long userId, long segmentId);

        Task<PostShard> GetPostAsync(long userId, long postId);
        Task<PostShard> AddPostAsync(long userId, long segmentId, MemoryStream image);
        Task DeletePostAsync(long userId, long postId);
    }

	#endregion
}

