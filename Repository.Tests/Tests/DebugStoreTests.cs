using Core.Boundaries;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Repository.Tests
{
    [Collection("Database Collection")]
    public class DebugStoreTests : IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;

        private static readonly EFCoreSentry sentry = new(Harbor.Flag.Development);
        private static readonly EFCoreDebugStore store = new(Harbor.Flag.Development);

        private User subject;

        public DebugStoreTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

            //subject = new UserFactory().Create();
            //sentry.ExecuteWrite(ctx => ctx.Users.Add(subject));
        }
        public void Dispose()
        {
            //sentry.ExecuteWrite(ctx => ctx.Users.ExecuteDelete());
        }

        [Fact]
        public async Task DrainDatabaseAsync_SUCCESS()
        {
            await store.DrainDatabaseAsync();

            int count = await sentry.ExecuteReadAsync(ctx => ctx.Users.CountAsync());

            Assert.Equal(0, count);
        }
    }
}

