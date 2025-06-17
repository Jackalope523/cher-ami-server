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

        Task<MemoryStream> DownloadPhotoAsync(long conversationId, Guid photoId);
        Task<Guid> UploadPhotoAsync(long conversationId, MemoryStream image);
        Task DeletePhotoAsync(long conversationId, Guid photoId);

        Task<MemoryStream> DownloadGroupChatHeaderAsync(long conversationId);
        Task UploadGroupChatHeaderAsync(long conversationId, MemoryStream image);
        Task DeleteGroupChatHeaderAsync(long conversationId);
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

        Task<MemoryStream> GetGroupChatHeaderAsync(long userId, long conversationId);
        Task<ImageMetadataShard> GetGroupChatHeaderMetadataAsync(long userId, long conversationId);

        Task<MemoryStream> GetPhotoAsync(long userId, long conversationId, Guid photoId);
        Task<ImageMetadataShard> GetPhotoMetadataAsync(long userId, long conversationId, Guid photoId);
    }

    #endregion
}
