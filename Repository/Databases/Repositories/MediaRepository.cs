using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Databases.Contexts;
using Repository.Databases.Entities;
using Repository.Databases.Entities.Messages;

namespace Repository.Databases.Stores
{
    public class MediaRepository : Repository, IMediaDatabase
    {
        private readonly string _storageAccountUri;
        private readonly Func<Azure.Core.TokenCredential> _credentials = () => new DefaultAzureCredential();

        internal MediaRepository(Func<CardinalContext> contextFactory, string storageAccountUri) : base(contextFactory)
        {
            _storageAccountUri = storageAccountUri;


        }

        private Uri BuildContainerUri(string containerName)
        {
            return new Uri($"{_storageAccountUri}/{containerName}");
        }

        private Uri BuildBlobUri(string relativePath)
        {
            return new Uri($"{_storageAccountUri}/{relativePath}");
        }

        private string BuildSnapshotBlobName(long circleId, long issueId, long postId, long snapshotId)
        {
            return $"{circleId}/issues/{issueId}/posts/{postId}/{snapshotId}.jpg";
        }

        private string BuildAvatarBlobName(long userId)
        {
            return $"{userId}/avatar/avatar.jpg";
        }

        private string BuildCircleHeaderBlobName(long circleId)
        {
            return $"{circleId}/header/header.jpg";
        }

        private string BuildPhotoBlobName(long chatId, long messageId)
        {
            return $"{chatId}/messages/{messageId}.jpg";
        }

        private static (string ContainerName, string BlobName) ParseBlobPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));

            string[] parts = path.Split(new[] { '/' }, 2);

            if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                throw new ArgumentException("Invalid blob path format. Expected format: 'container/blobName'", nameof(path));

            return (ContainerName: parts[0], BlobName: parts[1]);
        }

        private async Task UploadBlobAsync(string path, MemoryStream blob)
        {
            var splitPath = ParseBlobPath(path);

            BlobContainerClient containerClient = new(BuildContainerUri(splitPath.ContainerName), _credentials());

            await containerClient.CreateIfNotExistsAsync();
            blob.Position = 0;

            await containerClient.GetBlobClient(splitPath.BlobName).UploadAsync(blob, overwrite: true);
        }

        private async Task<MemoryStream> DownloadBlobAsync(string path)
        {
            MemoryStream stream = new MemoryStream();
            BlobClient blobClient = new(BuildBlobUri(path), _credentials());

            try
            {
                await blobClient.DownloadToAsync(stream);
                stream.Position = 0;
                return stream;
            }
            catch (Exception)
            {
                return await DownloadBlobAsync("utility/failed.jpg");
            }
        }

        private async Task DeleteBlobAsync(string path)
        {
            BlobClient blobClient = new(BuildBlobUri(path), _credentials());

            await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }

        public Task<MemoryStream> DownloadAssetAsync(string asset)
        {
            throw new NotImplementedException();
        }

        public async Task<MemoryStream> DownloadAvatarAsync(string path)
        {
             return await DownloadBlobAsync(path);
        }

        public async Task UploadAvatarAsync(long userId, MemoryStream image)
        {
            await using CardinalContext ctx = initContext();

            string path = BuildAvatarBlobName(userId);

            User toUpdate = new() { Id = userId, AvatarPath = path };
            ctx.Users.Attach(toUpdate);
            ctx.Entry(toUpdate).Property(nameof(User.AvatarPath)).IsModified = true;
            await ctx.SaveChangesAsync();

            await UploadBlobAsync(path, image);
        }

        public async Task DeleteAvatarAsync(string path)
        {
            await using CardinalContext ctx = initContext();

            await ctx.Users.
            Where(u => u.AvatarPath == path).
            ExecuteUpdateAsync(setters => setters.
            SetProperty(u => u.AvatarPath, ""));

            await DeleteBlobAsync(path);
        }

        public async Task<MemoryStream> DownloadCircleHeaderAsync(string path)
        {
            return await DownloadBlobAsync(path);
        }

        public async Task UploadCircleHeaderAsync(long circleId, MemoryStream image)
        {
            await using CardinalContext ctx = initContext();

            string path = BuildCircleHeaderBlobName(circleId);

            Circle toUpdate = new() { Id = circleId, HeaderPath = path };
            ctx.Circles.Attach(toUpdate);
            ctx.Entry(toUpdate).Property(nameof(Circle.HeaderPath)).IsModified = true;
            await ctx.SaveChangesAsync();

            await UploadBlobAsync(path, image);
        }

        public async Task DeleteCircleHeaderAsync(string path)
        {
            await using CardinalContext ctx = initContext();

            await ctx.Circles.
            Where(c => c.HeaderPath == path).
            ExecuteUpdateAsync(setters => setters.
            SetProperty(c => c.HeaderPath, ""));

            await DeleteBlobAsync(path);
        }

        public async Task<MemoryStream> DownloadSnapshotAsync(string path)
        {
            return await DownloadBlobAsync(path);
        }

        public async Task UploadSnapshotAsync(long circleId, long issueId, long postId, long snapshotId, MemoryStream image)
        {
            await using CardinalContext ctx = initContext();

            string path = BuildSnapshotBlobName(circleId, issueId, postId, snapshotId);

            Snapshot toUpdate = new() { Id = snapshotId, Path = path };
            ctx.Snapshots.Attach(toUpdate);
            ctx.Entry(toUpdate).Property(nameof(Snapshot.Path)).IsModified = true;
            await ctx.SaveChangesAsync();

            await UploadBlobAsync(path, image);
        }

        public async Task DeleteSnapshotAsync(string path)
        {
            await using CardinalContext ctx = initContext();

            await ctx.Snapshots.
            Where(s => s.Path == path).
            ExecuteUpdateAsync(setters => setters.
            SetProperty(s => s.Path, ""));

            await DeleteBlobAsync(path);
        }

        public async Task<MemoryStream> DownloadPhotoAsync(string path)
        {
            return await DownloadBlobAsync(path);
        }

        public async Task UploadPhotoAsync(long chatId, long messageId, MemoryStream image)
        {
            await using CardinalContext ctx = initContext();

            string path = BuildPhotoBlobName(chatId, messageId);

            PhotoMessage toUpdate = new() { Id = messageId, Path = path };
            ctx.PhotoMessages.Attach(toUpdate);
            ctx.Entry(toUpdate).Property(nameof(PhotoMessage.Path)).IsModified = true;
            await ctx.SaveChangesAsync();

            await UploadBlobAsync(path, image);
        }

        public async Task DeletePhotoAsync(string path)
        {
            await using CardinalContext ctx = initContext();

            await ctx.PhotoMessages.
            Where(m => m.Path == path).
            ExecuteUpdateAsync(setters => setters.
            SetProperty(m => m.Path, ""));

            await DeleteBlobAsync(path);
        }

        /* File Structure
         * /utility/failed.jpg
         * 
         * /users/{userId}/
         * ├── avatar/avatar.{ext}                        ← User profile picture
         * 
         * /circles/{circleId}/
         * ├── header/header.{ext}                          ← Circle header image
         * ├── issues/{issueId}/
         * │   ├── cover.{ext}                             ← Magazine issue header image
         * │   ├── posts/
         * │   │   ├── {postId}/
         * │   │   │   ├── {snapshotId}.{ext}         ← User-submitted image
         *
         * ├── chats/{chatId}
         * │   ├── messages/
         * │   │   ├── {message_id}.{ext}              ← Photos sent in chat messages     
        */
    }
}