using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Core.Boundaries;
using Microsoft.EntityFrameworkCore;
using Repository.Contexts;
using Repository.Entities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class MediaRepository(string storageAccountUri, LLContext ctx) : IMediaRepository
    {
        private readonly Func<Azure.Core.TokenCredential> _credentials = () => new DefaultAzureCredential();

        private async Task UploadBlobAsync(string path, MemoryStream blob)
        {
            string[] parts = path.Split(new[] { '/' }, 2);
            var (containerName, blobName) = (parts[0], parts[1]);

            BlobContainerClient containerClient = new(new Uri($"{storageAccountUri}/{containerName}"), _credentials());

            await containerClient.CreateIfNotExistsAsync();
            blob.Position = 0;

            await containerClient.GetBlobClient(blobName).UploadAsync(blob, overwrite: true);
        }

        private async Task<MemoryStream> DownloadBlobAsync(string path)
        {
            MemoryStream stream = new MemoryStream();
            BlobClient blobClient = new(new Uri($"{storageAccountUri}/{path}"), _credentials());

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
            BlobClient blobClient = new(new Uri($"{storageAccountUri}/{path}"), _credentials());

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
            string path = $"{userId}/avatar/avatar.jpg";

            User toUpdate = new() { Id = userId, AvatarPath = path };
            ctx.Users.Attach(toUpdate);
            ctx.Entry(toUpdate).Property(nameof(User.AvatarPath)).IsModified = true;
            await ctx.SaveChangesAsync();

            await UploadBlobAsync(path, image);
        }

        public async Task DeleteAvatarAsync(string path)
        {
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
            string path = $"{circleId}/header/header.jpg";

            Circle toUpdate = new() { Id = circleId, HeaderPath = path };
            ctx.Circles.Attach(toUpdate);
            ctx.Entry(toUpdate).Property(nameof(Circle.HeaderPath)).IsModified = true;
            await ctx.SaveChangesAsync();

            await UploadBlobAsync(path, image);
        }

        public async Task DeleteCircleHeaderAsync(string path)
        {
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
            string path = $"{circleId}/issues/{issueId}/posts/{postId}/{snapshotId}.jpg";

            Snapshot toUpdate = new() { Id = snapshotId, Path = path };
            ctx.Snapshots.Attach(toUpdate);
            ctx.Entry(toUpdate).Property(nameof(Snapshot.Path)).IsModified = true;
            await ctx.SaveChangesAsync();

            await UploadBlobAsync(path, image);
        }

        public async Task DeleteSnapshotAsync(string path)
        {
            await ctx.Snapshots.
            Where(s => s.Path == path).
            ExecuteUpdateAsync(setters => setters.
            SetProperty(s => s.Path, ""));

            await DeleteBlobAsync(path);
        }
    }
}