
namespace Repository
{
    internal class MediaStoreCoordinator: IMediaDatabase
    {
        private readonly IMediaDatabase store;

        public MediaStoreCoordinator(Harbor.Flag flag)
        {
            store = new AzureFileStore(flag);
        }

        public async Task<MemoryStream> DownloadAssetAsync(string asset)
        {
            return await store.DownloadAssetAsync(asset);
        }
        
        public async Task<MemoryStream> DownloadSnapshotAsync(long snapshotId, long ownerId)
        {
            return await store.DownloadSnapshotAsync(snapshotId, ownerId);
        }

        public async Task UploadSnapshotAsync(long snapshotId, long ownerId, MemoryStream image)
        {
            await store.UploadSnapshotAsync(snapshotId, ownerId, image);
        }

        public async Task DeleteSnapshotAsync(long snapshotId, long ownerId)
        {
            await store.DeleteSnapshotAsync(snapshotId, ownerId);
        }

        public async Task<MemoryStream> DownloadAvatarAsync(long userId)
        {
            return await store.DownloadAvatarAsync(userId);
        }

        public async Task UploadAvatarAsync(long userId, MemoryStream image)
        {
            await store.UploadAvatarAsync(userId, image);
        }

        public async Task DeleteAvatarAsync(long userId)
        {
            await store.DeleteAvatarAsync(userId);
        }

        public async Task<MemoryStream> DownloadGatheringHeaderAsync(long gatheringId)
        {
            return await store.DownloadGatheringHeaderAsync(gatheringId);
        }

        public async Task UploadGatheringHeaderAsync(long gatheringId, MemoryStream image)
        {
            await store.UploadGatheringHeaderAsync(gatheringId, image);
        }

        public async Task DeleteGatheringHeaderAsync(long gatheringId)
        {
            await store.DeleteGatheringHeaderAsync(gatheringId);
        }

        public async Task<MemoryStream> DownloadPhotoAsync(long conversationId, Guid photoId)
        {
            return await store.DownloadPhotoAsync(conversationId, photoId);
        }

        public async Task<Guid> UploadPhotoAsync(long conversationId, MemoryStream image)
        {
            return await store.UploadPhotoAsync(conversationId, image);
        }

        public async Task DeletePhotoAsync(long conversationId, Guid photoId)
        {
            await store.DeletePhotoAsync(conversationId, photoId);
        }

        public async Task<MemoryStream> DownloadGroupChatHeaderAsync(long conversationId)
        {
            return await store.DownloadGroupChatHeaderAsync(conversationId);
        }

        public async Task UploadGroupChatHeaderAsync(long conversationId, MemoryStream image)
        {
            await store.UploadGroupChatHeaderAsync(conversationId, image);
        }

        public async Task DeleteGroupChatHeaderAsync(long conversationId)
        {
            await store.DeleteGroupChatHeaderAsync(conversationId);
        }
    }
}
