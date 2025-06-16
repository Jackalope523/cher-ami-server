using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class EFCoreSnapshotStore : QueryStore, ISnapshotDatabase
    {   
        public EFCoreSnapshotStore(Harbor.Flag flag) : base(flag)
        {
        }

        public async Task<SnapshotShard> AddSnapshotAsync(long gatheringId, long posterId, DateTimeOffset timePosted)
        { 
            Snapshot toAdd = new() 
            { 
                GatheringId = gatheringId, 
                OwnerId = posterId, 
                PostedAt = timePosted
            };
            await storeSentry.ExecuteWriteAsync(ctx => ctx.Snapshots.Add(toAdd));

            string ownerName = await storeSentry.ExecuteReadAsync(ctx =>
                ctx.Users.
                Where(u => u.Id == posterId).
                Select(u => u.Name).
                SingleAsync());

            return new SnapshotShard ( toAdd.Id, toAdd.GatheringId, new UserShard(toAdd.OwnerId, ownerName), toAdd.PostedAt, 0);
        }

        public async Task<List<SnapshotShard>> GenerateColumnForUserAsync(long id, DateTimeOffset depthCharge, DateTimeOffset lastDepthCharge)
        {
            // Get List of Companions.
            Task<List<long>> appreciating = storeSentry.ExecuteReadAsync(ctx => 
                ctx.UserRelationships.
                Where(l => l.SelfId == id && l.Type == UserRelationship.UserRelationshipType.Follow).Select(l => l.OtherId).
                ToListAsync());

            Task<List<long>> appreciatingMe = storeSentry.ExecuteReadAsync(ctx => 
                ctx.UserRelationships.
                Where(l => l.OtherId == id && l.Type == UserRelationship.UserRelationshipType.Follow).
                Select(l => l.SelfId).
                ToListAsync());

            List<long> owners = (await appreciating).Intersect(await appreciatingMe).Append(id).ToList();

            // Get unseen posts by companions from certain depth.
            List<SnapshotShard> companionSnapshots = await storeSentry.ExecuteReadAsync(ctx =>
              ctx.Snapshots.
              Where(p => owners.Contains(p.OwnerId) && p.PostedAt >= depthCharge && p.PostedAt <= lastDepthCharge).
              Join(
                  ctx.Users,
                  p => p.OwnerId,
                  u => u.Id,
                  (p, u) => new SnapshotShard(p.Id, p.GatheringId, new(u.Id, u.Name), p.PostedAt, -1)
               ).ToListAsync());

            List<Task<int>> companionSnapshotsPositiveRatings = new();

            List<long> sitesToBeExplored = new();
            List<long> previouslyExtractedSnapshots = new();
            foreach (SnapshotShard s in companionSnapshots)
            {
                companionSnapshotsPositiveRatings.Add(storeSentry.ExecuteReadAsync(ctx => 
                    ctx.SnapshotLinks.
                    Where(l => l.SnapshotId == s.Id && l.Type == SnapshotLink.SnapshotLinkType.Appreciate).
                    CountAsync()));


                // Compile unique list if gatherings spanned by companion posts and a list of already loaded posts. 
                if (!sitesToBeExplored.Contains(s.GatheringId)) sitesToBeExplored.Add(s.GatheringId);
                previouslyExtractedSnapshots.Add(s.Id);
            }

            // Get remaining companion posts from same gatherings as others even if outside time range.
            List<SnapshotShard> nettedSnapshots = await storeSentry.ExecuteReadAsync(ctx => 
                ctx.Snapshots.
                Where(p => owners.Contains(p.OwnerId) && !previouslyExtractedSnapshots.Contains(p.Id) && sitesToBeExplored.Contains(p.GatheringId)).
                Join(
                  ctx.Users,
                  p => p.OwnerId,
                  u => u.Id,
                  (p, u) => new SnapshotShard(p.Id, p.GatheringId, new(u.Id, u.Name), p.PostedAt, -1)
                ).ToListAsync());

            List<Task<int>> nettedSnapshotsPositiveRatings = new();

            foreach (SnapshotShard s in nettedSnapshots)
            {
                nettedSnapshotsPositiveRatings.Add(storeSentry.ExecuteReadAsync(ctx =>
                    ctx.SnapshotLinks.
                    Where(l => l.SnapshotId == s.Id && l.Type == SnapshotLink.SnapshotLinkType.Appreciate).
                    CountAsync()));
            }

            for (int i = 0; i < companionSnapshots.Count; i++)
            {
                companionSnapshots[i] = companionSnapshots[i] with { Acclaim = await companionSnapshotsPositiveRatings[i] };
            }

            for (int i = 0; i < nettedSnapshots.Count; i++)
            {
                nettedSnapshots[i] = nettedSnapshots[i] with { Acclaim = await nettedSnapshotsPositiveRatings[i] };
            }

            return companionSnapshots.Concat(nettedSnapshots).ToList();

        }

        public async Task<SnapshotShard> GetSnapshotAsync(long id)
        {
            Task<int> appreciates = storeSentry.ExecuteReadAsync(ctx => ctx.SnapshotLinks.Where(l => l.SnapshotId == id && l.Type == SnapshotLink.SnapshotLinkType.Appreciate).CountAsync());

            SnapshotShard snapshot = await storeSentry.ExecuteReadAsync(ctx => 
            ctx.Snapshots.
            Where(p => p.Id == id).
            Select(p => new SnapshotShard(p.Id, p.GatheringId, new UserShard(p.OwnerId, null), p.PostedAt, 0)).
            SingleAsync());

            Task<string> name = storeSentry.ExecuteReadAsync(ctx => 
                ctx.Users.
                Where(u => u.Id == snapshot.User.Id).
                Select(u => u.Name).
                SingleAsync());

            return snapshot with { User = new UserShard(snapshot.User.Id, await name), Acclaim = await appreciates };
        }

        public async Task<List<SnapshotShard>> GetSnapshotsByUserAsync(long id)
        {
            List<SnapshotShard> snapshots = await storeSentry.ExecuteReadAsync(ctx =>
                 ctx.Snapshots.Where(p => p.OwnerId == id).
                 Select(a => new SnapshotShard(a.Id, a.GatheringId, new UserShard(a.OwnerId, null), a.PostedAt, 0)).
                 ToListAsync());

            List<Task<int>> positiveRatings = new(snapshots.Count);
            List<Task<string>> authorNames = new(snapshots.Count);
            for (int i = 0; i < snapshots.Count; i++)
            {
                positiveRatings.Add(storeSentry.ExecuteReadAsync(ctx => ctx.SnapshotLinks.Where(l => l.SnapshotId == snapshots[i].Id && l.Type == SnapshotLink.SnapshotLinkType.Appreciate).CountAsync()));
                authorNames.Add(storeSentry.ExecuteReadAsync(ctx => ctx.Users.Where(u => u.Id == snapshots[i].User.Id).Select(u => u.Name).SingleAsync()));
            }

            int[] ups = await Task.WhenAll(positiveRatings);
            string[] names = await Task.WhenAll(authorNames);

            for (int i = 0; i < snapshots.Count; i++)
            {
                snapshots[i] = snapshots[i] with { Acclaim = ups[i], User = new UserShard(snapshots[i].User.Id, names[i]) };
            }

            return snapshots;
        }

        public async Task AcclaimSnapshotAsync(long postId, long voterId)
        {
            SnapshotLink toAdd = new()
            {
                UserId = voterId,
                SnapshotId = postId,
                Time = DateTimeOffset.UtcNow,
                Type = SnapshotLink.SnapshotLinkType.Appreciate
            };

            long id = await storeSentry.ExecuteReadAsync(ctx =>
                        ctx.SnapshotLinks.
                        Where(l => l.UserId == voterId && l.SnapshotId == postId).
                        Select(l => l.Id).
                        SingleOrDefaultAsync());

            if (id != 0)
            {
                toAdd.Id = id;
            }

            await storeSentry.ExecuteWriteAsync(ctx => ctx.SnapshotLinks.Update(toAdd));
        }

        public async Task SoftDeleteAsync(long id)
        {
            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.SnapshotLinks.
               Where(l => l.SnapshotId == id).
               ExecuteUpdate(setter => setter.SetProperty(e => e.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.SnapshotReports.
               Where(r => r.SnapshotId == id).
               ExecuteUpdate(setter => setter.SetProperty(e => e.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
              ctx.Snapshots.
              Where(s => s.Id == id).
              ExecuteUpdate(setter => setter.SetProperty(e => e.SoftDeleted, true)));
        }

        public async Task HardDeleteAsync(long id)
        {
            await storeSentry.ExecuteWriteAsync(ctx => 
                ctx.SnapshotLinks.
                Where(l => l.SnapshotId == id).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.SnapshotReports.
                Where(r => r.SnapshotId == id).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx => 
                ctx.Snapshots.
                Remove(new Snapshot { Id = id }));
        }

        public async Task DeleteSnapshotAsync(long snapshotId)
        {
            await storeSentry.ExecuteWriteAsync(ctx => ctx.Snapshots.Remove(new Snapshot { Id = snapshotId }));
        }

        public async Task DeleteSnapshotAcclaimAsync(long postId, long voterId)
        {
            Func<CanaryContext, Task> query = EF.CompileAsyncQuery(
                (CanaryContext ctx) =>
                ctx.SnapshotLinks.
                Where(l => l.UserId == voterId && l.SnapshotId == postId).
                ExecuteDelete());

            await storeSentry.ExecuteWriteAsync(query);
        }

        public async Task<List<SnapshotShard>> GetSnapshotsForGatheringAsync(long id)
        {
            List<SnapshotShard> snapshots = await storeSentry.ExecuteReadAsync(ctx =>
                 ctx.Snapshots.Where(p => p.GatheringId == id).
                 Select(a => new SnapshotShard(a.Id, a.GatheringId, new UserShard(a.OwnerId, null), a.PostedAt, 0)).
                 ToListAsync());

            List<Task<int>> positiveRatings = new(snapshots.Count);
            List<Task<string>> authorNames = new(snapshots.Count);
            for (int i = 0; i < snapshots.Count; i++)
            {            
                positiveRatings.Add(storeSentry.ExecuteReadAsync(ctx => ctx.SnapshotLinks.Where(l => l.SnapshotId == snapshots[i].Id && l.Type == SnapshotLink.SnapshotLinkType.Appreciate).CountAsync()));
                authorNames.Add(storeSentry.ExecuteReadAsync(ctx => ctx.Users.Where(u => u.Id == snapshots[i].User.Id).Select(u => u.Name).SingleAsync()));
            }

            int[] ups = await Task.WhenAll(positiveRatings);
            string[] names = await Task.WhenAll(authorNames);

            for (int i = 0; i < snapshots.Count; i++)
            {
                snapshots[i] = snapshots[i] with { Acclaim = ups[i], User = new UserShard(snapshots[i].User.Id, names[i]) };
            }

            return snapshots;
        }
    }
}
