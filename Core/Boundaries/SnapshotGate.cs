using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace Core.Boundaries
{
	#region Schemas

    public enum SnapshotAcclaim
    { Acclaim, Remove }

	public record GatheringHeader(long Id, string Title, DateTimeOffset Time, bool IsActive, DateTimeOffset LastActiveTime,
        string FriendlyLocation);

    public record SnapshotShard(long Id, long GatheringId, UserShard User,
        DateTimeOffset TimeTaken, int Acclaim);

    public record ColumnShard(List<GatheringHeader> Headers, List<SnapshotShard> Snapshots);

    public record GalleryShard(List<SnapshotShard> Snapshots);

    #endregion

    #region Gates

    public interface ISnapshotDatabase
    {
        Task<List<SnapshotShard>> GetSnapshotsForGatheringAsync(long gatheringId);
        Task<List<SnapshotShard>> GetSnapshotsByUserAsync(long userId);
        Task<SnapshotShard> GetSnapshotAsync(long snapshotId);
        Task<SnapshotShard> AddSnapshotAsync(long gatheringId, long etcherId,
            DateTimeOffset timeTaken);

		Task AcclaimSnapshotAsync(long snapshotId, long voterId);
		Task DeleteSnapshotAcclaimAsync(long snapshotId, long voterId);

        Task<List<SnapshotShard>> GenerateColumnForUserAsync(long userId, DateTimeOffset depthCharge, DateTimeOffset lastDepth);

        Task SoftDeleteAsync(long snapshotId);
        Task HardDeleteAsync(long snapshotId);
    }

    public interface ISnapshotOperations
    {
        Task<SnapshotShard> GetSnapshotAsync(long userId, long snapshotId);
        Task<GalleryShard> GetGalleryAsync(long userId, long gatheringId);
        Task<SnapshotShard> AddSnapshotAsync(long userId, long gatheringId, MemoryStream image);
        Task DeleteSnapshotAsync(long userId, long snapshotId);
        Task AcclaimSnapshotAsync(long userId, long snapshotId, SnapshotAcclaim acclaim);

        Task<ColumnShard> GetWallAsync(long userId, int depth, int lastDepth);
    }

	#endregion
}

