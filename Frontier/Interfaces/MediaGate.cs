using System.Threading.Tasks;
using System.IO;
using System;
using CrazyLizard.Shared.Responses;

namespace Core.Boundaries
{

    #region Gates

    public interface IMediaRepository
    {
        Task<MemoryStream> DownloadAssetAsync(string asset);

        Task<MemoryStream> DownloadAvatarAsync(long userId);
        Task UploadAvatarAsync(long userId, MemoryStream image);
        Task DeleteAvatarAsync(long userId);

        Task<MemoryStream> DownloadCircleHeaderAsync(long circleId);
        Task UploadCircleHeaderAsync(long circleId, MemoryStream image);
        Task DeleteCircleHeaderAsync(long circleId);

        Task<MemoryStream> DownloadSnapshotAsync(long snapshotId);
        Task UploadSnapshotAsync(long snapshotId, MemoryStream image);
        Task DeleteSnapshotAsync(long snapshotId);
    }

    public interface IMediaService
    {
        Task<MemoryStream> GetAssetAsync(string asset);

        Task<MemoryStream> GetAvatarAsync(long requesterId, long userId);
        Task<ImageMetadataDTO> GetAvatarMetadataAsync(long requesterId, long userId);

        Task<MemoryStream> GetHeaderAsync(long requesterId, long circleId);
        Task<ImageMetadataDTO> GetHeaderMetadataAsync(long requesterId, long circleId);

        Task<MemoryStream> GetSnapshotAsync(long requesterId, long postId);
        Task<ImageMetadataDTO> GetSnapshotMetadataAsync(long requesterId, long postId);
    }

    #endregion
}
