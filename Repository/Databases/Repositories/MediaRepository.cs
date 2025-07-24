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

        private async Task UploadBlobAsync(string path, MemoryStream blob)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));

            string[] parts = path.Split(new[] { '/' }, 2);

            if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                throw new ArgumentException("Invalid blob path format. Expected format: 'container/blobName'", nameof(path));

            var (containerName, blobName) = (parts[0], parts[1]);

            BlobContainerClient containerClient = new(new Uri($"{_storageAccountUri}/{containerName}"), _credentials());

            await containerClient.CreateIfNotExistsAsync();
            blob.Position = 0;

            await containerClient.GetBlobClient(blobName).UploadAsync(blob, overwrite: true);
        }

        private async Task<MemoryStream> DownloadBlobAsync(string path)
        {
            MemoryStream stream = new MemoryStream();
            BlobClient blobClient = new(new Uri($"{_storageAccountUri}/{path}"), _credentials());

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
            BlobClient blobClient = new(new Uri($"{_storageAccountUri}/{path}"), _credentials());

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

            string path = $"{userId}/avatar/avatar.jpg";

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

            string path = $"{circleId}/header/header.jpg";

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

            string path = $"{circleId}/issues/{issueId}/posts/{postId}/{snapshotId}.jpg";

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

            string path = $"{chatId}/messages/{messageId}.jpg";

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
    }
}