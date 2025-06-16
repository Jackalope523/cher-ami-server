using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Core.Boundaries;
using Core.Entities;

using static Core.Entities.Arbiter;

namespace Core.Controls
{
    internal class MediaDirector : AbstractDirector, IMediaOperations
	{
		#region Initialisation

		public MediaDirector(CoreTerminal terminal) : base(terminal) { }

		#endregion

		#region Operations

		public async Task<MemoryStream> GetAssetAsync(string asset)
		{
			return await Media.DownloadAssetAsync(asset);
		}

		public async Task<MemoryStream> GetAvatarAsync(long userId, long targetId)
		{
			var user = await GetUserAsync(userId);
			User targetUser = new() { Id = targetId };

			FailIf(await user.IsBlockedBy(targetUser),
				new UserErrorException(UserErrorCode.CANNOT_VIEW));

            MemoryStream image;

            try
            {
                image = await Media.DownloadAvatarAsync(targetUser.Id);
            }
            catch (Exception ex)
            { throw new UnexpectedFailureException("", ex, HollowErrorCode.DOWNLOAD_FAILED); }

            return image;
		}

		public async Task<ImageMetadataShard> GetAvatarMetadataAsync(long userId, long targetId)
        {
            var user = await GetUserAsync(userId);
            User targetUser = new() { Id = targetId };

            FailIf(await user.IsBlockedBy(targetUser),
                new UserErrorException(UserErrorCode.CANNOT_VIEW));

            MemoryStream image;

            try
            {
                image = await Media.DownloadAvatarAsync(targetUser.Id);
            }
            catch (Exception ex)
            { throw new UnexpectedFailureException("", ex, HollowErrorCode.DOWNLOAD_FAILED); }

            // Get image hash
            var hashSync = ComputeHashAsync(image);

            // Check if image is concealed due to reports
            // TODO Does not yet exist for avatars

            return new(await hashSync, false);
        }

        public async Task<MemoryStream> GetHeaderAsync(long userId, long gatheringId)
        {
            var user = await GetUserAsync(userId);
			var gathering = await GetGatheringAsync(gatheringId);

            Verify(await gathering.IsVisibleTo(user),
                new UserErrorException(GatheringErrorCode.CANNOT_VIEW));

            MemoryStream image;

            try
            {
                image = await Media.DownloadGatheringHeaderAsync(gathering.Id);
            }
            catch (Exception ex)
            { throw new UnexpectedFailureException("", ex, HollowErrorCode.DOWNLOAD_FAILED); }

            return image;
        }

        public async Task<ImageMetadataShard> GetHeaderMetadataAsync(long userId, long gatheringId)
        {
            var user = await GetUserAsync(userId);
            var gathering = await GetGatheringAsync(gatheringId);

            Verify(await gathering.IsVisibleTo(user),
                new UserErrorException(GatheringErrorCode.CANNOT_VIEW));

            MemoryStream image;

            try
            {
                image = await Media.DownloadGatheringHeaderAsync(gathering.Id);
            }
            catch (Exception ex)
            { throw new UnexpectedFailureException("", ex, HollowErrorCode.DOWNLOAD_FAILED); }

            // Get image hash
            var hashSync = ComputeHashAsync(image);

            // Get reports
            var headerReports = (await gathering.GatheringReports)
                .FindAll(report => report.ReportType.Equals(GatheringReportType.InappropriateHeader));
            bool shouldConceal = false;

            // Check if header is concealed due to reports or if user reported it
            shouldConceal = headerReports.Count > 2 ||
                headerReports.Find(report => report.ReportingUserId.Equals(user.Id)) != default;

            return new(await hashSync, shouldConceal);
        }

        public async Task<MemoryStream> GetSnapshotAsync(long userId, long snapshotId)
        {
            var user = await GetUserAsync(userId);
            var snapshot = await Snapshots.GetSnapshotAsync(snapshotId);
            User snapshotOwner = await GetUserAsync(snapshot.User.Id);
            var etchedGathering = await GetGatheringAsync(snapshot.GatheringId);

            Verify(user.Taken(snapshot) ||
                await user.IsCompanionsWith(snapshotOwner) ||
                await etchedGathering.HasOnGuestList(user),
                new UserErrorException(SnapshotErrorCode.CANNOT_VIEW));

            MemoryStream image;

            try
            {
                image = await Media.DownloadSnapshotAsync(snapshot.Id, snapshot.User.Id);
            }
            catch (Exception ex)
            { throw new UnexpectedFailureException("", ex, HollowErrorCode.DOWNLOAD_FAILED); }

            return image;
        }

        public async Task<ImageMetadataShard> GetSnapshotMetadataAsync(long userId, long snapshotId)
        {
            var user = await GetUserAsync(userId);
            var snapshot = await Snapshots.GetSnapshotAsync(snapshotId);
            User snapshotOwner = await GetUserAsync(snapshot.User.Id);
            var etchedGathering = await GetGatheringAsync(snapshot.GatheringId);

            Verify(user.Taken(snapshot) ||
                await user.IsCompanionsWith(snapshotOwner) ||
                await etchedGathering.HasOnGuestList(user),
                new UserErrorException(SnapshotErrorCode.CANNOT_VIEW));

            MemoryStream image;

            try
            {
                image = await Media.DownloadSnapshotAsync(snapshot.Id, snapshot.User.Id);
            }
            catch (Exception ex)
            { throw new UnexpectedFailureException("", ex, HollowErrorCode.DOWNLOAD_FAILED); }

            // Get image hash
            var hashSync = ComputeHashAsync(image);

            // Get reports
            var reports = await Reports.GetReportsForSnapshotAsync(snapshot.Id);
            bool shouldConceal = false;

            // Check if image is concealed due to reports or if user reported it
            shouldConceal = reports.Count > 2 ||
                reports.Find(report => report.ReportingUserId.Equals(user.Id)) != default;

            return new(await hashSync, shouldConceal);
        }

        public async Task<MemoryStream> GetPhotoAsync(long userId, long conversationId, Guid photoId)
        {
            var user = await GetUserAsync(userId);
            var conversation = await GetConversationAsync(conversationId);

            Verify(await conversation.HasMember(user),
                new UserErrorException(UserErrorCode.CANNOT_VIEW));

            MemoryStream image;

            try
            {
                image = await Media.DownloadPhotoAsync(conversation.Id, photoId);
            }
            catch (Exception ex)
            { throw new UnexpectedFailureException("", ex, HollowErrorCode.DOWNLOAD_FAILED); }

            return image;
        }

        public async Task<ImageMetadataShard> GetPhotoMetadataAsync(long userId, long conversationId, Guid photoId)
        {
            var user = await GetUserAsync(userId);
            var conversation = await GetConversationAsync(conversationId);

            Verify(await conversation.HasMember(user),
                new UserErrorException(UserErrorCode.CANNOT_VIEW));

            MemoryStream image;

            try
            {
                image = await Media.DownloadPhotoAsync(conversation.Id, photoId);
            }
            catch (Exception ex)
            { throw new UnexpectedFailureException("", ex, HollowErrorCode.DOWNLOAD_FAILED); }

            // Get image hash
            var hashSync = ComputeHashAsync(image);

            return new(await hashSync, false);
        }

        public async Task<MemoryStream> GetGroupChatHeaderAsync(long userId, long conversationId)
        {
            var user = await GetUserAsync(userId);
            var conversation = await GetConversationAsync(conversationId);

            Verify(await conversation.HasMember(user),
                new UserErrorException(UserErrorCode.CANNOT_VIEW));

            MemoryStream image;

            try
            {
                image = await Media.DownloadGroupChatHeaderAsync(conversation.Id);
            }
            catch (Exception ex)
            { throw new UnexpectedFailureException("", ex, HollowErrorCode.DOWNLOAD_FAILED); }

            return image;
        }

        public async Task<ImageMetadataShard> GetGroupChatHeaderMetadataAsync(long userId, long conversationId)
        {
            var user = await GetUserAsync(userId);
            var conversation = await GetConversationAsync(conversationId);

            Verify(await conversation.HasMember(user),
                new UserErrorException(UserErrorCode.CANNOT_VIEW));

            MemoryStream image;

            try
            {
                image = await Media.DownloadGroupChatHeaderAsync(conversation.Id);
            }
            catch (Exception ex)
            { throw new UnexpectedFailureException("", ex, HollowErrorCode.DOWNLOAD_FAILED); }

            // Get image hash
            var hashSync = ComputeHashAsync(image);

            return new(await hashSync, false);
        }

        #endregion

        #region Favours

        public async Task UploadAvatarAsync(long userId, MemoryStream image)
		{
            try
            {
			    await Media.UploadAvatarAsync(userId, image);
            }
            catch (Exception ex)
            { throw new UnexpectedFailureException($"Failed to upload avatar for {userId}", ex, HollowErrorCode.UPLOAD_FAILED); }
		}

		public async Task UploadGatheringHeaderAsync(long gatheringId, MemoryStream image)
		{
            try
            {
                await Media.UploadGatheringHeaderAsync(gatheringId, image);
            }
            catch (Exception ex)
            { throw new UnexpectedFailureException($"Failed to upload gathering header for {gatheringId}", ex, HollowErrorCode.UPLOAD_FAILED); }
        }

        public async Task UploadSnapshotAsync(long userId, long snapshotId, MemoryStream image)
        {
            try
            {
                await Media.UploadSnapshotAsync(snapshotId, userId, image);
            }
            catch (Exception ex)
            { throw new UnexpectedFailureException($"Failed to upload snapshot for {snapshotId}", ex, HollowErrorCode.UPLOAD_FAILED); }
        }

        public async Task<Guid> UploadPhotoAsync(long conversationId, MemoryStream image)
		{
            try
            {
			    return await Media.UploadPhotoAsync(conversationId, image);
            }
            catch (Exception ex)
            { throw new UnexpectedFailureException($"Failed to upload photo for {conversationId}", ex, HollowErrorCode.UPLOAD_FAILED); }
        }

        public async Task UploadGroupChatHeaderAsync(long conversationId, MemoryStream image)
		{
            try
            {
			    await Media.UploadGroupChatHeaderAsync(conversationId, image);
            }
            catch (Exception ex)
            { throw new UnexpectedFailureException($"Failed to upload group chat header for {conversationId}", ex, HollowErrorCode.UPLOAD_FAILED); }
        }

        #endregion

        #region Tools

        public async Task<string> ComputeHashAsync(MemoryStream image)
        {
            if (image != null)
            {
                image.Seek(0, SeekOrigin.Begin);

                using SHA256 sha256 = SHA256.Create();

                byte[] hashBytes = await sha256.ComputeHashAsync(image);

                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                return hash;
            }
            else
            { throw new UnexpectedFailureException("Null image hash was unable to be computed.", code: HollowErrorCode.DOWNLOAD_FAILED); }
        }

        #endregion
    }
}
