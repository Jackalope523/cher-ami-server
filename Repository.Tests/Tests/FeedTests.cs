using Core.Boundaries;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Repository.Tests
{
    [Collection("Database Collection")]
    public class ColumnTests : IDisposable
    {
        private static EFCoreSentry sentry = new(Harbor.Flag.Development);
        private static EFCoreSnapshotStore store = new(Harbor.Flag.Development);

        private readonly ITestOutputHelper _testOutputHelper;

        private User subject;
        private Gathering testGathering;
        public ColumnTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

            subject = new UserFactory().Create();
            sentry.ExecuteWrite(ctx => ctx.Users.Add(subject));

            testGathering = new GatheringFactory().Create(subject);
            sentry.ExecuteWrite(ctx => ctx.Gatherings.Add(testGathering));
        }
        public void Dispose()
        {

        }


    
        [Fact]
        public async Task GenerateColumnForUserAsync_SUCCESS()
        {
            /// Column test layout
            /// subject companions: e, j, m
            /// bait: x
            ///                    depth charge
            ///                         v 
            /// Gathering | Time Z | Time A | Time B | Time C | Time D
            /// alpha |        | e1 j1  | e2 x1  | e3     | m1
            /// echo  |        | j2 x2  | j3     |        |
            /// hotel |        |        |        | e4 m2  |
            /// kilo  | e5     |        |        |        |
            /// romeo |        | j4     |        |        | m3 x3
            /// 
            /// previously seen: kilo { e5 }
            /// returned column: alpha { e1, j1, e2, e3, m1 }, echo { j2, j3 }, romeo { j4, m3 }
            /// 


            DateTimeOffset timeZ = DateTimeOffset.UtcNow;
            DateTimeOffset timeA = timeZ - TimeSpan.FromMinutes(60);
            DateTimeOffset timeB = timeA - TimeSpan.FromMinutes(60);
            DateTimeOffset depthCharge = timeA - TimeSpan.FromMinutes(90);
            DateTimeOffset timeC = timeA - TimeSpan.FromDays(120);
            DateTimeOffset timeD = timeA - TimeSpan.FromDays(180);


            // User creating block
            UserFactory userFactory = new UserFactory();
            User companionE = userFactory.Create();
            User companionJ = userFactory.Create();
            User companionM = userFactory.Create();
            User baitX = userFactory.Create();
            User irrelevantHost = userFactory.Create();

            sentry.ExecuteWrite(ctx => ctx.Users.Add(companionE));
            sentry.ExecuteWrite(ctx => ctx.Users.Add(companionJ));
            sentry.ExecuteWrite(ctx => ctx.Users.Add(companionM));
            sentry.ExecuteWrite(ctx => ctx.Users.Add(baitX));
            sentry.ExecuteWrite(ctx => ctx.Users.Add(irrelevantHost));


            // Companion making block
            UserLinkFactory factory = new UserLinkFactory();

            UserRelationship link1 = factory.Create(subject, companionE, UserRelationship.UserRelationshipType.Follow);
            UserRelationship link2 = factory.Create(companionE, subject, UserRelationship.UserRelationshipType.Follow);

            sentry.ExecuteWrite(ctx => ctx.UserRelationships.Add(link1));
            sentry.ExecuteWrite(ctx => ctx.UserRelationships.Add(link2));

            link1 = factory.Create(subject, companionJ, UserRelationship.UserRelationshipType.Follow);
            link2 = factory.Create(companionJ, subject, UserRelationship.UserRelationshipType.Follow);

            sentry.ExecuteWrite(ctx => ctx.UserRelationships.Add(link1));
            sentry.ExecuteWrite(ctx => ctx.UserRelationships.Add(link2));

            link1 = factory.Create(subject, companionM, UserRelationship.UserRelationshipType.Follow);
            link2 = factory.Create(companionM, subject, UserRelationship.UserRelationshipType.Follow);

            sentry.ExecuteWrite(ctx => ctx.UserRelationships.Add(link1));
            sentry.ExecuteWrite(ctx => ctx.UserRelationships.Add(link2));


            // Gathering block
            GatheringFactory gatheringFactory = new GatheringFactory();

            Gathering alpha = gatheringFactory.Create(irrelevantHost);
            Gathering echo = gatheringFactory.Create(irrelevantHost);
            Gathering hotel = gatheringFactory.Create(irrelevantHost);
            Gathering kilo = gatheringFactory.Create(irrelevantHost);
            Gathering romeo = gatheringFactory.Create(irrelevantHost);

            sentry.ExecuteWrite(ctx => ctx.Gatherings.Add(alpha));
            sentry.ExecuteWrite(ctx => ctx.Gatherings.Add(echo));
            sentry.ExecuteWrite(ctx => ctx.Gatherings.Add(hotel));
            sentry.ExecuteWrite(ctx => ctx.Gatherings.Add(kilo));
            sentry.ExecuteWrite(ctx => ctx.Gatherings.Add(romeo));


            // Post block
            // alpha
            Snapshot e1 = new SnapshotFactory().Create(companionE, alpha, timeA);
            Snapshot j1 = new SnapshotFactory().Create(companionJ, alpha, timeA);
            Snapshot e2 = new SnapshotFactory().Create(companionE, alpha, timeB);
            Snapshot x1 = new SnapshotFactory().Create(baitX, alpha, timeB);
            Snapshot e3 = new SnapshotFactory().Create(companionE, alpha, timeC);
            Snapshot m1 = new SnapshotFactory().Create(companionM, alpha, timeD);

            // echo
            Snapshot j2 = new SnapshotFactory().Create(companionJ, echo, timeA);
            Snapshot j3 = new SnapshotFactory().Create(companionJ, echo, timeB);
            Snapshot x2 = new SnapshotFactory().Create(baitX, echo, timeB);

            // hotel
            Snapshot e4 = new SnapshotFactory().Create(companionE, hotel, timeC);
            Snapshot m2 = new SnapshotFactory().Create(companionM, hotel, timeC);

            // kilo
            Snapshot e5 = new SnapshotFactory().Create(companionE, kilo, timeZ);

            // romeo
            Snapshot j4 = new SnapshotFactory().Create(companionJ, romeo, timeA);
            Snapshot m3 = new SnapshotFactory().Create(companionM, romeo, timeD);
            Snapshot x3 = new SnapshotFactory().Create(baitX, romeo, timeD);

            await BulkWritePost(e1, e2, e3, e4, e5, j1, j2, j3, j4, m1, m2, m3, x1, x2, x3);


            List<long> exclusionList = new() { kilo.Id };
            var retrieved = await store.GenerateColumnForUserAsync(subject.Id, depthCharge, timeA);


            Assert.NotNull(retrieved);
            Assert.Equal(9, retrieved.Count);

            SnapshotShard e1Snapshot = retrieved.Find(snapshot => snapshot.Id.Equals(e1.Id));
            Assert.NotNull(e1Snapshot);
            //Assert.Equal(e1.OwnerId, e1Snapshot.UserId);
            Assert.Equal(e1.GatheringId, e1Snapshot.GatheringId);
            Assert.Equal(e1.PostedAt, e1Snapshot.TimeTaken);

            var retrievedAsPostIds = retrieved.ConvertAll(snapshot => snapshot.Id);

            Assert.Contains(e1.Id, retrievedAsPostIds);
            Assert.Contains(e2.Id, retrievedAsPostIds);
            Assert.Contains(e3.Id, retrievedAsPostIds);
            Assert.Contains(j1.Id, retrievedAsPostIds);
            Assert.Contains(j2.Id, retrievedAsPostIds);
            Assert.Contains(j3.Id, retrievedAsPostIds);
            Assert.Contains(j4.Id, retrievedAsPostIds);
            Assert.Contains(m1.Id, retrievedAsPostIds);
            Assert.Contains(m3.Id, retrievedAsPostIds);
        }
        private async Task BulkWritePost(params Snapshot[] posts)
        {
            foreach (var post in posts)
            {
                sentry.ExecuteWrite(ctx => ctx.Snapshots.Add(post));
            }
        }    
    }
}
