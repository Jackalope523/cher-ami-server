using Core.Boundaries;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Xunit.Abstractions;


namespace Repository.Tests
{
    [Collection("Database Collection")]
    public class NestStoreTests : IDisposable
    {
        private static EFCoreSentry sentry = new(Harbor.Flag.Development);
        private static EFCoreNestStore nestStore = new(Harbor.Flag.Development);

        private readonly ITestOutputHelper _testOutputHelper;

        private User subject1;
        private User subject2;
        public NestStoreTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

            UserFactory userFactory = new UserFactory();
            subject1 = userFactory.Create();
            subject2 = userFactory.Create();

            sentry.ExecuteWrite(ctx => ctx.Users.Add(subject1));
            sentry.ExecuteWrite(ctx => ctx.Users.Add(subject2));
        }
        public void Dispose()
        {
            sentry.ExecuteWrite(ctx => ctx.UserRelationships.ExecuteDelete());
            sentry.ExecuteWrite(ctx => ctx.Users.ExecuteDelete());
        }

        [Fact]
        public async Task AppreciateUserAsync_SUCCESS()
        {
            DateTimeOffset time = DateTimeOffset.UtcNow;

            await nestStore.FollowUserAsync(subject1.Id, subject2.Id, time);

            UserRelationship link = sentry.ExecuteRead(ctx => ctx.UserRelationships.Where(l => l.SelfId == subject1.Id && l.OtherId == subject2.Id).Single());

            Assert.NotNull(link);
            Assert.Equal(subject1.Id, link.SelfId);
            Assert.Equal(subject2.Id, link.OtherId);
            Assert.Equal(time, link.Time);
            Assert.Equal(UserRelationship.UserRelationshipType.Follow, link.Type);
        }
        [Fact]
        public async Task UnappreciateUserAsync_SUCCESS()
        {
            UserRelationship link = new UserLinkFactory().Create(subject1, subject2, UserRelationship.UserRelationshipType.Follow);
            sentry.ExecuteWrite(ctx => ctx.UserRelationships.Add(link));

            await nestStore.UnfollowUserAsync(subject1.Id, subject2.Id);

            int count = sentry.ExecuteRead(ctx => ctx.UserRelationships.Count());

            Assert.Equal(0, count);
        }
        [Fact]
        public async Task BlockUserAsync_SUCCESS()
        {
            DateTimeOffset time = DateTimeOffset.UtcNow;

            await nestStore.BlockUserAsync(subject1.Id, subject2.Id, time);

            UserRelationship link = sentry.ExecuteRead(ctx => ctx.UserRelationships.Where(l => l.SelfId == subject1.Id && l.OtherId == subject2.Id).Single());

            Assert.NotNull(link);
            Assert.Equal(subject1.Id, link.SelfId);
            Assert.Equal(subject2.Id, link.OtherId);
            Assert.Equal(time, link.Time);
            Assert.Equal(UserRelationship.UserRelationshipType.Block, link.Type);
        }
        [Fact]
        public async Task UnblockUserAsync_SUCCESS()
        {
            UserRelationship link = new UserLinkFactory().Create(subject1, subject2, UserRelationship.UserRelationshipType.Block);
            sentry.ExecuteWrite(ctx => ctx.UserRelationships.Add(link));

            await nestStore.UnblockUserAsync(subject1.Id, subject2.Id);

            int numLinks = await sentry.ExecuteReadAsync(ctx => ctx.UserRelationships.CountAsync());

            Assert.Equal(0, numLinks);
        }
        [Fact]
        public async Task GetAppreciatedUsersAsync_SUCCESS()
        {
            UserRelationship link = new UserLinkFactory().Create(subject1, subject2, UserRelationship.UserRelationshipType.Follow);
            await sentry.ExecuteWriteAsync(ctx => ctx.UserRelationships.AddAsync(link));

            CoreUser user = (await nestStore.GetFollowedUsersAsync(subject1.Id)).Single();

            Assert.NotNull(user);
            Assert.Equal(subject2.Id, user.Id);
            Assert.Equal(subject2.Name, user.Name);
        }
        [Fact]
        public async Task GetBlockedUsersAsync_SUCCESS()
        {
            UserRelationship link = new UserLinkFactory().Create(subject1, subject2, UserRelationship.UserRelationshipType.Block);
            await sentry.ExecuteWriteAsync(ctx => ctx.UserRelationships.Add(link));

            UserShard user = (await nestStore.GetBlockedUsersAsync(subject1.Id)).Single();

            Assert.NotNull(user);
            Assert.Equal(subject2.Id, user.Id);
            Assert.Equal(subject2.Name, user.Name);
        }
        [Fact]
        public async Task GetCompanionsAsync_SUCCESS()
        {
            UserLinkFactory factory = new UserLinkFactory();
            UserRelationship link1 = factory.Create(subject1, subject2, UserRelationship.UserRelationshipType.Follow);
            UserRelationship link2 = factory.Create(subject2, subject1, UserRelationship.UserRelationshipType.Follow);

            sentry.ExecuteWrite(ctx => ctx.UserRelationships.Add(link1));
            sentry.ExecuteWrite(ctx => ctx.UserRelationships.Add(link2));

            CoreUser user = (await nestStore.GetCompanionsAsync(subject1.Id)).Single();

            Assert.NotNull(user);
            Assert.Equal(subject2.Id, user.Id);
            Assert.Equal(subject2.Name, user.Name);
        }
        [Fact]
        public async Task GetUsersAppreciatingAsync_SUCCESS()
        {
            UserRelationship link = new UserLinkFactory().Create(subject2, subject1, UserRelationship.UserRelationshipType.Follow);
            sentry.ExecuteWrite(ctx => ctx.UserRelationships.Add(link));

            CoreUser user = (await nestStore.GetUserFollowersAsync(subject1.Id)).Single();

            Assert.NotNull(user);
            Assert.Equal(subject2.Id, user.Id);
            Assert.Equal(subject2.Name, user.Name);
        }
        [Fact]
        public async Task GetUsersBlockingAsync_SUCCESS()
        {
            UserRelationship link = new UserLinkFactory().Create(subject2, subject1, UserRelationship.UserRelationshipType.Block);
            sentry.ExecuteWrite(ctx => ctx.UserRelationships.Add(link));

            CoreUser user = (await nestStore.GetUsersBlockingAsync(subject1.Id)).Single();

            Assert.NotNull(user);
            Assert.Equal(subject2.Id, user.Id);
            Assert.Equal(subject2.Name, user.Name);
        }
    }
}