using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Core.Boundaries;
using static LazyLizardBackend.Arbiter;

namespace LazyLizardBackend.Services
{
    public class MediaService(IMediaRepository mediaRepository) : IMediaService
	{
        public Task<MemoryStream> GetAssetAsync(string asset)
        {
            throw new NotImplementedException();
        }

        public Task<MemoryStream> GetAvatarAsync(long userId, long targetId)
        {
            throw new NotImplementedException();
        }

        public Task<ImageMetadataDTO> GetAvatarMetadataAsync(long userId, long targetId)
        {
            throw new NotImplementedException();
        }

        public Task<MemoryStream> GetHeaderAsync(long userId, long circleId)
        {
            throw new NotImplementedException();
        }

        public Task<ImageMetadataDTO> GetHeaderMetadataAsync(long userId, long circleId)
        {
            throw new NotImplementedException();
        }

        public Task<MemoryStream> GetPhotoAsync(long userId, long chatId, Guid photoId)
        {
            throw new NotImplementedException();
        }

        public Task<ImageMetadataDTO> GetPhotoMetadataAsync(long userId, long chatId, Guid photoId)
        {
            throw new NotImplementedException();
        }

        public Task<MemoryStream> GetPostAsync(long userId, long postId)
        {
            throw new NotImplementedException();
        }

        public Task<ImageMetadataDTO> GetPostMetadataAsync(long userId, long postId)
        {
            throw new NotImplementedException();
        }
    }
}
