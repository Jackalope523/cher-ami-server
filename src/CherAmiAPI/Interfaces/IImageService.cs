using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CherAmiAPI.Interfaces
{
    public interface IImageService
    {
        public Task UploadImageAsync(string path, MemoryStream image);
        public Task<MemoryStream> DownloadImageAsync(string path);
        public Task DeleteImageAsync(string path);
        public Task DeleteImagesAsync(List<string> paths);
    }
}
