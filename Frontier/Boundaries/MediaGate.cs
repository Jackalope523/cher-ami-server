using System.Threading.Tasks;
using System.IO;
using System;
using LazyLizardBackend.Shared.Responses;

namespace Core.Boundaries
{

    #region Gates

    public interface IMediaRepository
    {
        Task<MemoryStream> DownloadAssetAsync(string asset);

        Task<MemoryStream> DownloadAvatarAsync(string path);
        Task UploadAvatarAsync(long userId, MemoryStream image);
        Task DeleteAvatarAsync(string path);

        Task<MemoryStream> DownloadCircleHeaderAsync(string path);
        Task UploadCircleHeaderAsync(long circleId, MemoryStream image);
        Task DeleteCircleHeaderAsync(string path);

        Task<MemoryStream> DownloadSnapshotAsync(string path);
        Task UploadSnapshotAsync(long circleId, long issueId, long postId, long snapshotId, MemoryStream image);
        Task DeleteSnapshotAsync(string path);
    }

    public interface IMediaService
    {
        Task<MemoryStream> GetAssetAsync(string asset);

        Task<MemoryStream> GetAvatarAsync(long userId, long targetId);
        Task<ImageMetadataDTO> GetAvatarMetadataAsync(long userId, long targetId);

        Task<MemoryStream> GetHeaderAsync(long userId, long circleId);
        Task<ImageMetadataDTO> GetHeaderMetadataAsync(long userId, long circleId);

        Task<MemoryStream> GetPostAsync(long userId, long postId);
        Task<ImageMetadataDTO> GetPostMetadataAsync(long userId, long postId);

        Task<MemoryStream> GetPhotoAsync(long userId, long chatId, Guid photoId);
        Task<ImageMetadataDTO> GetPhotoMetadataAsync(long userId, long chatId, Guid photoId);
    }

    #endregion
}
