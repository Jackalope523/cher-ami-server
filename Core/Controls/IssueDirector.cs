using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.Boundaries;
using Core.Entities;

using static Core.Entities.Arbiter;
using static Core.Entities.Psijic;

namespace Core.Controls
{
    internal class IssueDirector : AbstractDirector, IIssueOperations
	{
		#region Initialisation

		public IssueDirector(CoreTerminal terminal) : base(terminal) { }

		#endregion

		#region Operations

        public async Task<PostShard> GetPostAsync(long userId, long snapshotId)
        {
            var user = await GetUserAsync(userId);
            var snapshot = await Snapshots.GetPostAsync(snapshotId);

            var snapshotOwner = await GetUserAsync(snapshot.User.Id);
            var gathering = await GetGatheringAsync(snapshot.GatheringId);

            // Fail if user is blocked
            FailIf(await user.IsBlockedBy(snapshotOwner),
                new UserErrorException(UserErrorCode.CANNOT_VIEW));

            // Fail if user cannot view gathering
            Verify(await user.CanView(gathering),
                new UserErrorException(GatheringErrorCode.CANNOT_VIEW));

            return snapshot;
        }

        public async Task<GalleryShard> GetPostsForIssueAsync(long userId, long gatheringId)
        {
            var user = await GetUserAsync(userId);
            var gathering = await GetGatheringAsync(gatheringId);

            // Fail if user cannot view gathering
            Verify(await user.CanView(gathering),
                new UserErrorException(GatheringErrorCode.CANNOT_VIEW));

            GalleryShard gallery = new(new());

            if (await gathering.HasOnGuestList(user))
            {
                // Remove any snapshots from blocked or blocking users
                GalleryShard filteredGallery = new(await RemoveBlockedSnapshotsAsync(user, await gathering.Posts));

                // Remove strangers if gallery is in pre mode
                if (gathering.IsUpcoming)
                {
                    var strangers = await Nests.ReturnStrangerDangerAsync(user.Id, filteredGallery.Snapshots.Select(snapshot => snapshot.User.Id).ToArray());

                    strangers.Remove(gathering.HostId);
                    strangers.Remove(user.Id);

                    gallery = new(HideStrangersAsync(filteredGallery.Snapshots, strangers));
                }
            }
            // Check if any companions attended
            else
            {
                var companionIds = (await user.Companions)
                    .ConvertAll(companion => companion.Id);

                var companionSnapshots = (await gathering.Posts)
                    .Where(snapshot => companionIds.Contains(snapshot.User.Id)).ToList();

                gallery = new(companionSnapshots);
            }

            return gallery;
        }


        public async Task<PostShard> AddPostAsync(long userId, long gatheringId, MemoryStream image)
        {
            var userSync = GetUserAsync(userId);
            var targetGatheringSync = GetGatheringAsync(gatheringId);
            var user = await userSync;
            var gathering = await targetGatheringSync;

            await user.CanPostTo(gathering);

            // Try to etch
            var snapshot = await Snapshots.AddPostAsync(gathering.Id, user.Id, Time);

            try
            {
                // Save image
                await Terminal.MediaDirector.UploadSnapshotAsync(user.Id, snapshot.Id, image);
            }
            catch (Exception ex)
            {
                // If failed, remove snapshot
                await Snapshots.HardDeleteAsync(snapshot.Id);
                throw new UnexpectedFailureException($"Failed to upload snapshot for user {userId} at {gatheringId}.", ex, HollowErrorCode.UPLOAD_FAILED);
            }

            return snapshot;
        }

        public async Task DeletePostAsync(long userId, long snapshotId)
        {
            var userSync = GetUserAsync(userId);
            var snapshot = await Snapshots.GetPostAsync(snapshotId);
            var gatheringTaken = await GetGatheringAsync(snapshot.GatheringId);
            var user = await userSync;

            // Verify user owns the snapshot or can modify the gathering
            Verify(user.Owns(snapshot) || gatheringTaken.IsModifiableBy(user),
                new UserErrorException(SnapshotErrorCode.CANNOT_DELETE));

            await Snapshots.SoftDeleteAsync(snapshot.Id);
        }

        public async Task AcclaimSnapshotAsync(long userId, long snapshotId, SnapshotAcclaim acclaim)
        {
            var userSync = GetUserAsync(userId);
            var snapshot = await Snapshots.GetPostAsync(snapshotId);
            var gatheringTaken = await GetGatheringAsync(snapshot.GatheringId);
            var user = await userSync;

            // Verify user can interact with snapshot
            Verify(await gatheringTaken.WasAttendedBy(user),
                new UserErrorException(SnapshotErrorCode.CANNOT_INTERACT));

            FailIf(user.Owns(snapshot),
                new UserErrorException(SnapshotErrorCode.CANNOT_INTERACT_SELF));

            // Check action
            if (acclaim == SnapshotAcclaim.Acclaim)
            {
                await Snapshots.AcclaimSnapshotAsync(snapshot.Id, user.Id);
            }
            else
            {
                await Snapshots.DeleteSnapshotAcclaimAsync(snapshot.Id, user.Id);
            }
        }

        public async Task<ColumnShard>
            GetWallAsync(long userId, int depth, int lastDepth)
        {
            var user = await GetUserAsync(userId);
            Dictionary<long, GatheringHeader> gatheringHeaders = new();

            // Enforce lastDepth < depth
            lastDepth = Math.Min(lastDepth, depth - 1);

            // Retrieve companion-populated gathering snapshots after a specified time excluding previously viewed gatherings
            DateTimeOffset depthCharge = Time - TimeSpan.FromDays(depth);
            DateTimeOffset lastDepthCharge = Time - TimeSpan.FromDays(lastDepth);
            var generatedWall = await Snapshots.GenerateColumnForUserAsync(user.Id, depthCharge, lastDepthCharge);

            // Get the respective gathering headers for the snapshots
            foreach (var snapshot in generatedWall)
            {
                var gatheringId = snapshot.GatheringId;

                // Add gathering header if it does not yet exist
                if (!gatheringHeaders.ContainsKey(gatheringId))
                {
                    var etchedGathering = await GetGatheringAsync(gatheringId);

                    gatheringHeaders.Add(gatheringId, etchedGathering.ToGatheringHeader(snapshot.TimeTaken));
                }
                // Update gathering header active time if snapshot is more recent
                else if (HappenedBefore(gatheringHeaders[gatheringId].LastActiveTime, snapshot.TimeTaken))
                {
                    gatheringHeaders[gatheringId] = gatheringHeaders[gatheringId] with { LastActiveTime = snapshot.TimeTaken };
                }
            }

            return new(gatheringHeaders.Values.ToList(), generatedWall);
        }

		#endregion

		#region Favours

		internal async Task<List<PostShard>> RequestGatheringSnapshotsAsync(Issue gathering)
            => await Snapshots.GetPostsForIssueAsync(gathering.Id);

		internal async Task<List<PostShard>> RequestVisibleSnapshotsAsync(User user, Issue gathering)
        {
            // Verify user can see the gathering
            if (!await user.CanView(gathering))
            {
                return new List<PostShard> { };
            }

            _ = gathering.Posts.Sync();

            // Determine the level of visibility to the user

            // Check if the user attended the gathering
            if (await gathering.WasAttendedBy(user) || gathering.IsModifiableBy(user))
            {
                return await gathering.Posts;
            }
            else
            {
                // Get all companion snapshots
                var companions = await user.Companions;

                var companionSnapshots = (await gathering.Posts)
                    .Where(snapshot => companions.Exists(cmp => cmp.Id.Equals(snapshot.User.Id)))
                    .ToList();

                return companionSnapshots;
            }
        }

        internal List<PostShard>
            HideStrangersAsync(List<PostShard> snapshots, List<long> strangers)
        {
            List<PostShard> collection = new();

            foreach (PostShard snapshot in snapshots)
            {
                if (strangers.Contains(snapshot.User.Id))
                {
                    collection.Add(new(snapshot.Id, snapshot.GatheringId,
                        User.Hidden.ToUserShard(),
                        snapshot.TimeTaken, snapshot.Acclaim));
                }
                else
                {
                    collection.Add(snapshot);
                }
            }

            return collection;
        }

        internal async Task<List<PostShard>>
            RemoveBlockedSnapshotsAsync(User user, List<PostShard> snapshots)
        {
            List<PostShard> accessibleSnapshots = new();

            foreach (PostShard snapshot in snapshots)
            {
                if (user.Id != snapshot.User.Id)
                {
                    User snapshotOwner = await GetUserAsync(snapshot.User.Id);

                    // Check if blocking link exists
                    if (await user.IsBlocking(snapshotOwner) || await user.IsBlockedBy(snapshotOwner))
                    { continue; }
                }

                accessibleSnapshots.Add(snapshot);
            }

            return accessibleSnapshots;
        }

        #endregion
    }
}

