using Azure.Identity;
using CrazyLizard.Contexts;
using CrazyLizard.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CrazyLizard.Repositories
{
    public class MediaRepository(string storageAccountUri, ApplicationDbContext ctx) : IMediaRepository
    {
        private readonly Func<Azure.Core.TokenCredential> _credentials = () => new DefaultAzureCredential();

        private readonly string _baseFolder = Path.Combine(AppContext.BaseDirectory, "localstorage");

        public async Task UploadBlobAsync(string path, MemoryStream blob)
        {
            //string[] parts = path.Split(new[] { '/' }, 2);
            //var (containerName, blobName) = (parts[0], parts[1]);

            //BlobContainerClient containerClient = new(new Uri($"{storageAccountUri}/{containerName}"), _credentials());

            //await containerClient.CreateIfNotExistsAsync();
            //blob.Position = 0;

            //await containerClient.GetBlobClient(blobName).UploadAsync(blob, overwrite: true);

            string fullPath = Path.Combine(_baseFolder, path);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            blob.Position = 0;
            using (FileStream fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                await blob.CopyToAsync(fileStream);
            }
        }

        public async Task<MemoryStream> DownloadBlobAsync(string path)
        {
            //MemoryStream stream = new MemoryStream();
            //BlobClient blobClient = new(new Uri($"{storageAccountUri}/{path}"), _credentials());

            //try
            //{
            //    await blobClient.DownloadToAsync(stream);
            //    stream.Position = 0;
            //    return stream;
            //}
            //catch (Exception)
            //{
            //    return await DownloadBlobAsync("utility/failed.jpg");
            //}

            string fullPath = Path.Combine(_baseFolder, path);

            try
            {
                MemoryStream stream = new MemoryStream();
                using (FileStream fileStream = new(fullPath, FileMode.Open, FileAccess.Read))
                {
                    await fileStream.CopyToAsync(stream);
                }
                stream.Position = 0;
                return stream;
            }
            catch (Exception)
            {
                return await DownloadBlobAsync("utility/failed.jpg");
            }
        }

        public async Task DeleteBlobAsync(string path)
        {
            //BlobClient blobClient = new(new Uri($"{storageAccountUri}/{path}"), _credentials());

            //await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);

            string fullPath = Path.Combine(_baseFolder, path);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
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
            string path = $"users/{userId}/avatar.jpg";

            await ctx.Users.
            Where(u => u.Id == userId).
            ExecuteUpdateAsync(setters => setters.
            SetProperty(u => u.AvatarPath, path));

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
            string path = $"circles/{circleId}/header/header.jpg";

            await ctx.Circles.
            Where(x => x.Id == circleId).
            ExecuteUpdateAsync(setters => setters.
            SetProperty(c => c.HeaderPath, path));

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

        public async Task<MemoryStream> DownloadPostImageAsync(long postId)
        {
            string path = await ctx.Posts.Where(x => x.Id == postId).Select(x => x.ImagePath).SingleAsync();
            return await DownloadBlobAsync(path);
        }

        public async Task UploadPostImageAsync(long postId, MemoryStream image)
        {
            long issueId = await ctx.Posts.Where(x => x.Id == postId).Select(x => x.IssueId).SingleAsync();
            long circleId = await ctx.Issues.Where(x => x.Id == issueId).Select(x => x.CircleId).SingleAsync();

            string path = $"circles/{circleId}/issues/{issueId}/posts/{postId}.jpg";

            await ctx.Posts.
            Where(x => x.Id == postId).
            ExecuteUpdateAsync(setters => setters.
            SetProperty(x => x.ImagePath, path));

            await UploadBlobAsync(path, image);
        }

        public async Task DeleteSnapshotAsync(long snapshotId)
        {
            string path = await ctx.Posts.Where(x => x.Id == snapshotId).Select(x => x.ImagePath).SingleAsync();

            await ctx.Posts.
            Where(s => s.ImagePath == path).
            ExecuteUpdateAsync(setters => setters.
            SetProperty(s => s.ImagePath, ""));

            await DeleteBlobAsync(path);
        }
    }
}