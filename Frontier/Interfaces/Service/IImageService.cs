using System.IO;
using System.Threading.Tasks;

namespace CrazyLizard.Interfaces.Service
{
    public interface IImageService
    {
        public Task UploadImageAsync(string path, MemoryStream image);
        public Task<MemoryStream> DownloadImageAsync(string path);
        public Task DeleteImageAsync(string path);
    }
}
