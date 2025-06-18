using System.Threading.Tasks;
using System.IO;
using System;

namespace Core.Boundaries
{
    #region Schemas

    public record ImageMetadataShard(string Hash, bool Concealed);

    #endregion

    #region Gates

    public interface IMediaDatabase
    {
        Task<MemoryStream> DownloadAssetAsync(string asset);

        Task<MemoryStream> DownloadAvatarAsync(long userId);
        Task UploadAvatarAsync(long userId, MemoryStream image);
        Task DeleteAvatarAsync(long userId);

        Task<MemoryStream> DownloadGroupHeaderAsync(long groupId);
        Task UploadGroupHeaderAsync(long groupId, MemoryStream image);
        Task DeleteGroupHeaderAsync(long groupId);

        Task<MemoryStream> DownloadPostAsync(long postId, long ownerId);
        Task UploadPostAsync(long postId, long ownerId, MemoryStream image);
        Task DeletePostAsync(long postId, long ownerId);

        Task<MemoryStream> DownloadPhotoAsync(long chatId, Guid photoId);
        Task<Guid> UploadPhotoAsync(long chatId, MemoryStream image);
        Task DeletePhotoAsync(long chatId, Guid photoId);
    }

    public interface IMediaOperations
    {
        Task<MemoryStream> GetAssetAsync(string asset);

        Task<MemoryStream> GetAvatarAsync(long userId, long targetId);
        Task<ImageMetadataShard> GetAvatarMetadataAsync(long userId, long targetId);

        Task<MemoryStream> GetHeaderAsync(long userId, long groupId);
        Task<ImageMetadataShard> GetHeaderMetadataAsync(long userId, long groupId);

        Task<MemoryStream> GetPostAsync(long userId, long postId);
        Task<ImageMetadataShard> GetPostMetadataAsync(long userId, long postId);

        Task<MemoryStream> GetPhotoAsync(long userId, long chatId, Guid photoId);
        Task<ImageMetadataShard> GetPhotoMetadataAsync(long userId, long chatId, Guid photoId);
    }

    #endregion
}
