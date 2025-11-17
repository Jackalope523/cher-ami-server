using CherAmiAPI.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CherAmiAPI.Services
{
    public class FileSystemImageService : IImageService
    {
        private readonly string _baseFolder = Path.Combine(AppContext.BaseDirectory, "localstorage");

        public async Task UploadImageAsync(string path, MemoryStream image)
        {
            string fullPath = Path.Combine(_baseFolder, path);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            image.Position = 0;
            using (FileStream fileStream = new(fullPath, FileMode.Create, FileAccess.Write))
            {
                await image.CopyToAsync(fileStream);
            }
        }

        public async Task<MemoryStream> DownloadImageAsync(string path)
        {
            string fullPath = Path.Combine(_baseFolder, path);

            MemoryStream stream = new MemoryStream();
            using (FileStream fileStream = new(fullPath, FileMode.Open, FileAccess.Read))
            {
                await fileStream.CopyToAsync(stream);
            }
            stream.Position = 0;
            return stream;
        }

        public async Task DeleteImageAsync(string path)
        {
            string fullPath = Path.Combine(_baseFolder, path);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
