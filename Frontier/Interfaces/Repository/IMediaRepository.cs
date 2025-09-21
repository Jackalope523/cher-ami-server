using System.Threading.Tasks;
using System.IO;

namespace CrazyLizard.Interfaces.Repository
{
    public interface IMediaRepository
    {
        Task<MemoryStream> DownloadAssetAsync(string asset);

        Task<MemoryStream> DownloadAvatarAsync(long userId);
        Task UploadAvatarAsync(long userId, MemoryStream image);
        Task DeleteAvatarAsync(long userId);

        Task<MemoryStream> DownloadCircleHeaderAsync(long circleId);
        Task UploadCircleHeaderAsync(long circleId, MemoryStream image);
        Task DeleteCircleHeaderAsync(long circleId);

        Task<MemoryStream> DownloadPostImageAsync(long postId);
        Task UploadPostImageAsync(long postId, MemoryStream image);
        Task DeleteSnapshotAsync(long postId);
    }
}
