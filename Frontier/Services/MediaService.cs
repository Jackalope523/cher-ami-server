using System;
using System.IO;
using System.Threading.Tasks;
using CrazyLizard.Boundaries.Repository;
using CrazyLizard.Exceptions;
using CrazyLizard.Interfaces.Repository;
using CrazyLizard.Interfaces.Service;
using CrazyLizard.Shared.Responses;

namespace CrazyLizard.Services
{
    public class MediaService(IAccountRepository accountRepository, ICircleRepository circleRepository, IMediaRepository mediaRepository) : IMediaService
    {
        public Task<MemoryStream> GetAssetAsync(string asset)
        {
            throw new NotImplementedException();
        }

        public async Task<MemoryStream> GetAvatarAsync(long requesterId, long userId)
        {
            if (!await accountRepository.ShareCircle(requesterId, userId))
                throw new NoAccessException($"User {requesterId} and user {userId} do not share a circle.");

            return await mediaRepository.DownloadAvatarAsync(userId);
        }

        public Task<ImageMetadataDTO> GetAvatarMetadataAsync(long requesterId, long userId)
        {
            throw new NotImplementedException();
        }

        public async Task<MemoryStream> GetHeaderAsync(long requesterId, long circleId)
        {
            if (!await circleRepository.IsMemberAsync(requesterId, circleId))
                throw new NoAccessException($"User {requesterId} is not a member of circle {circleId}.");

            return await mediaRepository.DownloadCircleHeaderAsync(circleId);
        }

        public Task<ImageMetadataDTO> GetHeaderMetadataAsync(long requesterId, long circleId)
        {
            throw new NotImplementedException();
        }

        public Task<MemoryStream> GetSnapshotAsync(long requesterId, long postId)
        {
            throw new NotImplementedException();
        }

        public Task<ImageMetadataDTO> GetSnapshotMetadataAsync(long requesterId, long postId)
        {
            throw new NotImplementedException();
        }
    }
}
