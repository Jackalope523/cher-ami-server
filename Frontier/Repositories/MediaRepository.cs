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
using User = Repository.Entities.User;

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

        public async Task<MemoryStream> DownloadAvatarAsync(long userId)
        {
            string path = await ctx.Users.Where(x => x.Id == userId).Select(x => x.AvatarPath).SingleAsync();
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

        public async Task DeleteAvatarAsync(long userId)
        {
            string path = await ctx.Users.Where(x => x.Id == userId).Select(x => x.AvatarPath).SingleAsync();

            await ctx.Users.
            Where(u => u.Id == userId).
            ExecuteUpdateAsync(setters => setters.
            SetProperty(u => u.AvatarPath, ""));

            await DeleteBlobAsync(path);
        }

        public async Task<MemoryStream> DownloadCircleHeaderAsync(long circleId)
        {
            string path = await ctx.Circles.Where(x => x.Id == circleId).Select(x => x.HeaderPath).SingleAsync();
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

        public async Task DeleteCircleHeaderAsync(long circleId)
        {
            string path = await ctx.Circles.Where(x => x.Id == circleId).Select(x => x.HeaderPath).SingleAsync();

            await ctx.Circles.
            Where(c => c.HeaderPath == path).
            ExecuteUpdateAsync(setters => setters.
            SetProperty(c => c.HeaderPath, ""));

            await DeleteBlobAsync(path);
        }

        public async Task<MemoryStream> DownloadSnapshotAsync(long snapshotId)
        {
            string path = await ctx.Snapshots.Where(x => x.Id == snapshotId).Select(x => x.Path).SingleAsync();
            return await DownloadBlobAsync(path);
        }

        public async Task UploadSnapshotAsync(long snapshotId, MemoryStream image)
        {
            long postId = await ctx.Snapshots.Where(x => x.Id == snapshotId).Select(x => x.PostId).SingleAsync();
            long issueId = await ctx.Posts.Where(x => x.Id == postId).Select(x => x.IssueId).SingleAsync();
            long circleId = await ctx.Issues.Where(x => x.Id == issueId).Select(x => x.CircleId).SingleAsync();

            string path = $"{circleId}/issues/{issueId}/posts/{postId}/{snapshotId}.jpg";

            Snapshot toUpdate = new() { Id = snapshotId, Path = path };
            ctx.Snapshots.Attach(toUpdate);
            ctx.Entry(toUpdate).Property(nameof(Snapshot.Path)).IsModified = true;
            await ctx.SaveChangesAsync();

            await UploadBlobAsync(path, image);
        }

        public async Task DeleteSnapshotAsync(long snapshotId)
        {
            string path = await ctx.Snapshots.Where(x => x.Id == snapshotId).Select(x => x.Path).SingleAsync();

            await ctx.Snapshots.
            Where(s => s.Path == path).
            ExecuteUpdateAsync(setters => setters.
            SetProperty(s => s.Path, ""));

            await DeleteBlobAsync(path);
        }
    }
}