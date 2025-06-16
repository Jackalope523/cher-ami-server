using Core.Boundaries;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Repository.Tests
{
    [Collection("Database Collection")]
    public class SnapshotStoreTests : IDisposable
    {
        private static EFCoreSentry sentry = new(Harbor.Flag.Development);
        private static EFCoreSnapshotStore snapshotStore = new(Harbor.Flag.Development);

        private readonly ITestOutputHelper _testOutputHelper;

        private User subject;
        private Gathering testGathering;
        public SnapshotStoreTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

            subject = new UserFactory().Create();
            sentry.ExecuteWrite(ctx => ctx.Users.Add(subject));

            testGathering = new GatheringFactory().Create(subject);
            sentry.ExecuteWrite(ctx => ctx.Gatherings.Add(testGathering));
        }
        public void Dispose()
        {
            sentry.ExecuteWrite(ctx => ctx.SnapshotLinks.ExecuteDelete());
            sentry.ExecuteWrite(ctx => ctx.Snapshots.ExecuteDelete());
            sentry.ExecuteWrite(ctx => ctx.Users.ExecuteDelete());
            sentry.ExecuteWrite(ctx => ctx.Gatherings.ExecuteDelete());
        }

        [Fact]
        public async Task AddSnapshotAsync_SUCCESS()
        {
            DateTimeOffset postTime = DateTimeOffset.MinValue;
            string url = "URL";

            await snapshotStore.AddSnapshotAsync(testGathering.Id, subject.Id, postTime);

            Snapshot created = await sentry.ExecuteReadAsync(ctx => ctx.Snapshots.FirstAsync());

            Assert.NotNull(created);
            Assert.Equal(subject.Id, created.OwnerId);
            Assert.Equal(testGathering.Id, created.GatheringId);
            Assert.Equal(postTime, created.PostedAt);
        }
        [Fact]
        public async Task GetSnapshotAsync_SUCCESS()
        {
            Snapshot testSnapshot = new SnapshotFactory().Create(subject, testGathering);
            await sentry.ExecuteWriteAsync(ctx => ctx.Snapshots.AddAsync(testSnapshot));

            SnapshotShard retrieved = await snapshotStore.GetSnapshotAsync(testSnapshot.Id);

            Assert.NotNull(retrieved);
            Assert.Equal(testSnapshot.OwnerId, retrieved.User.Id);
            Assert.Equal(testSnapshot.GatheringId, retrieved.GatheringId);
            Assert.Equal(testSnapshot.PostedAt, retrieved.TimeTaken);
        }
        [Fact]
        public async Task GetSnapshotsByUserAsync_SUCCESS()
        {
            Snapshot testSnapshot = new SnapshotFactory().Create(subject, testGathering);
            sentry.ExecuteWrite(ctx => ctx.Snapshots.Add(testSnapshot));

            int a = sentry.ExecuteRead(ctx => ctx.Snapshots.Count());
            _testOutputHelper.WriteLine(a.ToString());

           SnapshotShard retrieved = (await snapshotStore.GetSnapshotsByUserAsync(subject.Id)).First();

            Assert.NotNull(retrieved);
            Assert.Equal(testSnapshot.OwnerId, retrieved.User.Id);
            Assert.Equal(testSnapshot.GatheringId, retrieved.GatheringId);
            Assert.Equal(testSnapshot.PostedAt, retrieved.TimeTaken);
        }
        [Fact]
        public async Task GetSnapshotsForGatheringAsync_SUCCESS()
        {
            Snapshot testSnapshot = new SnapshotFactory().Create(subject, testGathering);
            sentry.ExecuteWrite(ctx => ctx.Snapshots.Add(testSnapshot));

            SnapshotShard retrieved = (await snapshotStore.GetSnapshotsForGatheringAsync(testGathering.Id)).First();

            Assert.NotNull(retrieved);
            Assert.Equal(testSnapshot.OwnerId, retrieved.User.Id);
            Assert.Equal(testSnapshot.GatheringId, retrieved.GatheringId);
            Assert.Equal(testSnapshot.PostedAt, retrieved.TimeTaken);
        }
        [Fact]
        public async Task RateSnapshotAsync_SUCCESS()
        {
            SnapshotLink.SnapshotLinkType rating = SnapshotLink.SnapshotLinkType.Appreciate;
            Snapshot testSnapshot = new SnapshotFactory().Create(subject, testGathering);
            sentry.ExecuteWrite(ctx => ctx.Snapshots.Add(testSnapshot));

            await snapshotStore.AcclaimSnapshotAsync(testSnapshot.Id, subject.Id);

            SnapshotLink created = await sentry.ExecuteReadAsync(ctx => ctx.SnapshotLinks.FirstAsync());

            Assert.Equal(subject.Id, created.UserId);
            Assert.Equal(testSnapshot.Id, created.SnapshotId);
            Assert.Equal(rating, created.Type);
        }
        [Fact]
        public async Task DeleteSnapshotRatingAsync_SUCCESS()
        {
            Snapshot testSnapshot = new SnapshotFactory().Create(subject, testGathering);
            await sentry.ExecuteWriteAsync(ctx => ctx.Snapshots.AddAsync(testSnapshot));

            SnapshotLink rating = new PostLinkFactory().Create(subject, testSnapshot);
            await sentry.ExecuteWriteAsync(ctx => ctx.SnapshotLinks.AddAsync(rating));

            await snapshotStore.DeleteSnapshotAcclaimAsync(testSnapshot.Id, subject.Id);

            int count = await sentry.ExecuteReadAsync(ctx => ctx.SnapshotLinks.CountAsync());

            Assert.Equal(0, count);
        }
    }
}

