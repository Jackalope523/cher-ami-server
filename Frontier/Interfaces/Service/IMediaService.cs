using System.Threading.Tasks;
using System.IO;
using CrazyLizard.Shared.Responses;

namespace CrazyLizard.Interfaces.Service
{
    public interface IMediaService
    {
        Task<MemoryStream> GetAssetAsync(string asset);

        Task<MemoryStream> GetAvatarAsync(long requesterId, long userId);
        Task<ImageMetadataDTO> GetAvatarMetadataAsync(long requesterId, long userId);

        Task<MemoryStream> GetHeaderAsync(long requesterId, long circleId);
        Task<ImageMetadataDTO> GetHeaderMetadataAsync(long requesterId, long circleId);

        Task<MemoryStream> GetSnapshotAsync(long requesterId, long postId);
        Task<ImageMetadataDTO> GetSnapshotMetadataAsync(long requesterId, long postId);
    }
}
