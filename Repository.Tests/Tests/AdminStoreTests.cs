using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Repository.Tests
{
    [Collection("Database Collection")]
    public class AdminStoreTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        private static readonly EFCoreSentry sentry = new(Harbor.Flag.Development);
        private static readonly AdminStoreCoordinator store = new(Harbor.Flag.Development);

        private User subject;

        public AdminStoreTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

            subject = new UserFactory().Create();
            sentry.ExecuteWrite(ctx => ctx.Users.Add(subject));
        }
        public void Dispose()
        {
            sentry.ExecuteWrite(ctx => ctx.Users.ExecuteDelete());
        }
    }
}
