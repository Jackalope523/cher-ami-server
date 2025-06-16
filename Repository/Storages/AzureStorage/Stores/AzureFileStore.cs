using Core.Boundaries;
using Serilog;

namespace Repository
{
    public class AzureFileStore : IMediaDatabase
    {
        private readonly AzureStorageSentry sentry;

        private readonly string userContainerPrefix = "user";
        private readonly string gatheringContainerPrefix = "gathering";
        private readonly string conversationContainerPrefix = "conversation";

        private readonly string imageFileSuffix = ".jpg";

        public AzureFileStore(Harbor.Flag flag) 
        {
            sentry = new(flag);
        }

        public async Task<MemoryStream> DownloadAssetAsync(string asset)
        {
            return await sentry.DownloadBlobAsync("assets", asset + imageFileSuffix);
        }

        public async Task UploadSnapshotAsync(long snapshotId, long ownerId, MemoryStream image)
        {
            await sentry.UploadBlobAsync(userContainerPrefix + ownerId.ToString(), snapshotId.ToString() + imageFileSuffix, image);
        }

        public async Task<MemoryStream> DownloadSnapshotAsync(long snapshotId, long ownerId)
        {
            return await sentry.DownloadBlobAsync(userContainerPrefix + ownerId.ToString(), snapshotId.ToString() + imageFileSuffix);
        }

        public async Task DeleteSnapshotAsync(long snapshotId, long ownerId)
        {
            await sentry.DeleteBlobAsync(userContainerPrefix + ownerId.ToString(), snapshotId.ToString() + imageFileSuffix);
        }

        public async Task<MemoryStream> DownloadAvatarAsync(long userId)
        {
            return await sentry.DownloadBlobAsync(userContainerPrefix + userId.ToString(), "avatar" + imageFileSuffix);
        }

        public async Task UploadAvatarAsync(long userId, MemoryStream image)
        {
            await sentry.UploadBlobAsync(userContainerPrefix + userId.ToString(), "avatar" + imageFileSuffix, image);
        }

        public async Task DeleteAvatarAsync(long userId)
        {
            await sentry.DeleteBlobAsync(userContainerPrefix + userId.ToString(), "avatar" + imageFileSuffix);
        }

        public async Task<MemoryStream> DownloadGatheringHeaderAsync(long gatheringId)
        {
            return await sentry.DownloadBlobAsync(gatheringContainerPrefix + gatheringId.ToString(), "hero" + imageFileSuffix);
        }

        public async Task UploadGatheringHeaderAsync(long gatheringId, MemoryStream image)
        {
            await sentry.UploadBlobAsync(gatheringContainerPrefix + gatheringId.ToString(), "hero" + imageFileSuffix, image);
        }

        public async Task DeleteGatheringHeaderAsync(long gatheringId)
        {
            await sentry.DeleteBlobAsync(gatheringContainerPrefix + gatheringId.ToString(), "hero" + imageFileSuffix);
        }

        public async Task<MemoryStream> DownloadPhotoAsync(long conversationId, Guid photoId)
        {
            return await sentry.DownloadBlobAsync(conversationContainerPrefix + conversationId.ToString(), photoId.ToString() + imageFileSuffix);
        }

        public async Task<Guid> UploadPhotoAsync(long conversationId, MemoryStream image)
        {
            Guid photoId = Guid.NewGuid();

            await sentry.UploadBlobAsync(conversationContainerPrefix + conversationId.ToString(), photoId.ToString() + imageFileSuffix, image);

            return photoId;
        }

        public async Task DeletePhotoAsync(long conversationId, Guid photoId)
        {
            await sentry.DeleteBlobAsync(conversationContainerPrefix + conversationId.ToString(), photoId.ToString() + imageFileSuffix);
        }

        public async Task<MemoryStream> DownloadGroupChatHeaderAsync(long conversationId)
        {
            return await sentry.DownloadBlobAsync(conversationContainerPrefix + conversationId.ToString(), "header" + imageFileSuffix);
        }

        public async Task UploadGroupChatHeaderAsync(long conversationId, MemoryStream image)
        {
            await sentry.UploadBlobAsync(conversationContainerPrefix + conversationId.ToString(), "header" + imageFileSuffix, image);
        }

        public async Task DeleteGroupChatHeaderAsync(long conversationId)
        {
            await sentry.DeleteBlobAsync(conversationContainerPrefix + conversationId.ToString(), "header" + imageFileSuffix);
        }
    }
}
