using Core.Boundaries;
using Microsoft.EntityFrameworkCore;
using Repository.Entities;

using Xunit.Abstractions;

namespace Repository.Tests
{
    [Collection("Database Collection")]
    public class NotificationStoreTests: IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;

        private static EFCoreSentry sentry = new(Harbor.Flag.Development);
        private static EFCoreNotificationStore store = new(Harbor.Flag.Development);

        private User subject1;
        private User subject2;

        public NotificationStoreTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

            UserFactory factory = new UserFactory();
            subject1 = factory.Create();
            subject2 = factory.Create();

            sentry.ExecuteWrite(ctx => ctx.Users.Add(subject1));
            sentry.ExecuteWrite(ctx => ctx.Users.Add(subject2));

        }
        public void Dispose()
        {
            sentry.ExecuteWrite(ctx => ctx.Telegrams.ExecuteDelete());
            sentry.ExecuteWrite(ctx => ctx.Subscriptions.ExecuteDelete());
            sentry.ExecuteWrite(ctx => ctx.Users.ExecuteDelete());
        }
    }
}
