using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Core.Boundaries;
using Core.Entities;

using static Core.Entities.Arbiter;

namespace Core.Controls
{
    public class MediaService : AbstractService, IMediaOperations
	{
        public MediaService(CoreTerminal terminal) : base(terminal) { }

        public Task<MemoryStream> GetAssetAsync(string asset)
        {
            throw new NotImplementedException();
        }

        public Task<MemoryStream> GetAvatarAsync(long userId, long targetId)
        {
            throw new NotImplementedException();
        }

        public Task<ImageMetadataShard> GetAvatarMetadataAsync(long userId, long targetId)
        {
            throw new NotImplementedException();
        }

        public Task<MemoryStream> GetHeaderAsync(long userId, long circleId)
        {
            throw new NotImplementedException();
        }

        public Task<ImageMetadataShard> GetHeaderMetadataAsync(long userId, long circleId)
        {
            throw new NotImplementedException();
        }

        public Task<MemoryStream> GetPhotoAsync(long userId, long chatId, Guid photoId)
        {
            throw new NotImplementedException();
        }

        public Task<ImageMetadataShard> GetPhotoMetadataAsync(long userId, long chatId, Guid photoId)
        {
            throw new NotImplementedException();
        }

        public Task<MemoryStream> GetPostAsync(long userId, long postId)
        {
            throw new NotImplementedException();
        }

        public Task<ImageMetadataShard> GetPostMetadataAsync(long userId, long postId)
        {
            throw new NotImplementedException();
        }
    }
}
