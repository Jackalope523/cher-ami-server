using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CrazyLizard.Interfaces.Service;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CherAmiAPI.Services
{
    public class AzureImageService : IImageService
    {
        private readonly Func<Azure.Core.TokenCredential> _credentials = () => new DefaultAzureCredential();
        private readonly string storageAccountUri = "https://stcheramidataprod.blob.core.windows.net";

        public async Task UploadImageAsync(string path, MemoryStream blob)
        {
            string[] parts = path.Split(new[] { '/' }, 2);
            var (containerName, blobName) = (parts[0], parts[1]);

            BlobContainerClient containerClient = new(new Uri($"{storageAccountUri}/{containerName}"), _credentials());

            await containerClient.CreateIfNotExistsAsync();
            blob.Position = 0;

            await containerClient.GetBlobClient(blobName).UploadAsync(blob, overwrite: true);
        }

        public async Task<MemoryStream> DownloadImageAsync(string path)
        {
            MemoryStream stream = new MemoryStream();
            BlobClient blobClient = new(new Uri($"{storageAccountUri}/{path}"), _credentials());

            await blobClient.DownloadToAsync(stream);
            stream.Position = 0;
            return stream;
        }

        public async Task DeleteImageAsync(string path)
        {
            BlobClient blobClient = new(new Uri($"{storageAccountUri}/{path}"), _credentials());

            await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }
    }
}
