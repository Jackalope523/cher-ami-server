using Core.Boundaries;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Repository.Tests
{
    [Collection("Database Collection")]
    public class DisciplineStoreTests : IDisposable
    {
        private static EFCoreSentry sentry = new(Harbor.Flag.Development);
        private static EFCoreDisciplineStore store = new(Harbor.Flag.Development);

        private readonly ITestOutputHelper _testOutputHelper;

        private User subject1;
        private User subject2;
        private Gathering testGathering;

        public DisciplineStoreTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

            UserFactory userFactory = new UserFactory();
            subject1 = userFactory.Create();
            subject2 = userFactory.Create();

            sentry.ExecuteWrite(ctx => ctx.Users.Add(subject1));
            sentry.ExecuteWrite(ctx => ctx.Users.Add(subject2));

            GatheringFactory gatheringFactory = new GatheringFactory();
            testGathering = gatheringFactory.Create(subject2);

            sentry.ExecuteWrite(ctx => ctx.Gatherings.Add(testGathering));
        }
        public void Dispose()
        {
            sentry.ExecuteWrite(ctx => ctx.Penalties.ExecuteDelete());
            sentry.ExecuteWrite(ctx => ctx.UserReports.ExecuteDelete());
            sentry.ExecuteWrite(ctx => ctx.GatheringReports.ExecuteDelete());
            sentry.ExecuteWrite(ctx => ctx.Users.ExecuteDelete());
            sentry.ExecuteWrite(ctx => ctx.Gatherings.ExecuteDelete());
        }

        [Fact]
        public async Task GetReportsByUserAsync_SUCCESS()
        {
            UserReport userReport = new UserReportFactory().Create(subject1, subject2, testGathering);
            GatheringReport gatheringReport = new GatheringReportFactory().Create(subject1, testGathering);

            await sentry.ExecuteWriteAsync(ctx => ctx.UserReports.Add(userReport));
            await sentry.ExecuteWriteAsync(ctx => ctx.GatheringReports.Add(gatheringReport));

            (List<Core.Boundaries.UserReport>, List<Core.Boundaries.GatheringReport>, List<Core.Boundaries.SnapshotReport>) reports = await store.GetReportsByUserAsync(subject1.Id);

            Assert.NotNull(reports.Item1);
            Assert.NotNull(reports.Item2);

            Assert.Equal(userReport.SelfId, reports.Item1.First().ReportingUserId);
            Assert.Equal(userReport.OtherId, reports.Item1.First().ReportedUserId);
            Assert.Equal(userReport.FilingDate, reports.Item1.First().ReportTime);
            Assert.Equal(userReport.Type, reports.Item1.First().ReportType);
            Assert.Equal(userReport.Notes, reports.Item1.First().ReportDetails);

            Assert.Equal(gatheringReport.UserId, reports.Item2.First().ReportingUserId);
            Assert.Equal(gatheringReport.GatheringId, reports.Item2.First().ReportedGatheringId);
            Assert.Equal(gatheringReport.FilingDate, reports.Item2.First().ReportTime);
            Assert.Equal(gatheringReport.Type, reports.Item2.First().ReportType);
            Assert.Equal(gatheringReport.Notes, reports.Item2.First().ReportDetails);
        }
        [Fact]
        public async Task ReportUserAsync_SUCCESS()
        {
            string notes = "Test";
            UserReportType type = UserReportType.Rude;
            DateTimeOffset time = DateTimeOffset.UtcNow;

            await store.ReportUserAsync(subject1.Id, testGathering.Id, subject2.Id, time, type, notes);

            UserReport created = await sentry.ExecuteReadAsync(ctx => ctx.UserReports.FirstAsync());

            Assert.NotNull(created);
            Assert.Equal(subject1.Id, created.SelfId);
            Assert.Equal(subject2.Id, created.OtherId);
            Assert.Equal(testGathering.Id, created.GatheringId);
            Assert.Equal(time, created.FilingDate);
            Assert.Equal(type, created.Type);
            Assert.Equal(notes, created.Notes);
        }
        [Fact]
        public async Task ReportGatheringAsync_SUCCESS()
        {
            string notes = "Test";
            GatheringReportType type = GatheringReportType.InappropriateHeader;
            DateTimeOffset time = DateTimeOffset.UtcNow;

            await store.ReportGatheringAsync(subject1.Id, testGathering.Id, time, type, notes);

            GatheringReport created = await sentry.ExecuteReadAsync(ctx => ctx.GatheringReports.FirstAsync());

            Assert.NotNull(created);
            Assert.Equal(subject1.Id, created.UserId);
            Assert.Equal(testGathering.Id, created.GatheringId);
            Assert.Equal(time, created.FilingDate);
            Assert.Equal(type, created.Type);
            Assert.Equal(notes, created.Notes);
        }
        [Fact]
        public async Task GetReportsForGatheringAsync_SUCCESS()
        {
            GatheringReport gatheringReport = new GatheringReportFactory().Create(subject1, testGathering);
            await sentry.ExecuteWriteAsync(ctx => ctx.GatheringReports.Add(gatheringReport));

            List<Core.Boundaries.GatheringReport> reports = await store.GetReportsForGatheringAsync(testGathering.Id);

            Assert.NotNull(reports);
            Assert.Equal(gatheringReport.UserId, reports.First().ReportingUserId);
            Assert.Equal(gatheringReport.GatheringId, reports.First().ReportedGatheringId);
            Assert.Equal(gatheringReport.FilingDate, reports.First().ReportTime);
            Assert.Equal(gatheringReport.Type, reports.First().ReportType);
            Assert.Equal(gatheringReport.Notes, reports.First().ReportDetails);
        }
        [Fact]
        public async Task GetReportsForUserAsync_SUCCESS()
        {
            UserReport userReport = new UserReportFactory().Create(subject1, subject2, testGathering);
            GatheringReport gatheringReport = new GatheringReportFactory().Create(subject1, testGathering);

            await sentry.ExecuteWriteAsync(ctx => ctx.UserReports.Add(userReport));
            await sentry.ExecuteWriteAsync(ctx => ctx.GatheringReports.Add(gatheringReport));

            (List<Core.Boundaries.UserReport>, List<Core.Boundaries.GatheringReport>, List<Core.Boundaries.SnapshotReport>) reports = await store.GetReportsForUserAsync(subject2.Id);

            Assert.NotNull(reports.Item1);
            Assert.NotNull(reports.Item2);

            Assert.Equal(userReport.SelfId, reports.Item1.First().ReportingUserId);
            Assert.Equal(userReport.OtherId, reports.Item1.First().ReportedUserId);
            Assert.Equal(userReport.FilingDate, reports.Item1.First().ReportTime);
            Assert.Equal(userReport.Type, reports.Item1.First().ReportType);
            Assert.Equal(userReport.Notes, reports.Item1.First().ReportDetails);

            Assert.Equal(gatheringReport.UserId, reports.Item2.First().ReportingUserId);
            Assert.Equal(gatheringReport.GatheringId, reports.Item2.First().ReportedGatheringId);
            Assert.Equal(gatheringReport.FilingDate, reports.Item2.First().ReportTime);
            Assert.Equal(gatheringReport.Type, reports.Item2.First().ReportType);
            Assert.Equal(gatheringReport.Notes, reports.Item2.First().ReportDetails);
        }
        [Fact]
        public async Task PenaliseUserAsync_SUCCESS()
        {
            await store.PenaliseUserAsync(subject1.Id, PenaltyType.Unreliable, DateTimeOffset.MinValue);

            Penalty penalty = sentry.ExecuteRead(ctx => ctx.Penalties.Single());

            Assert.NotNull(penalty);
            Assert.Equal(subject1.Id, penalty.PenalizedId);
            Assert.Equal(DateTimeOffset.MinValue, penalty.Time);
            Assert.Equal(PenaltyType.Unreliable, penalty.Type);
        }
        [Fact]
        public async Task GetPenaltiesForUserAsync_SUCCESS()
        {
            Penalty penalty = new PenaltyFactory().Create(subject1);
            sentry.ExecuteWrite(ctx => ctx.Penalties.Add(penalty));

            PenaltyShard found = (await store.GetPenaltiesForUserAsync(subject1.Id)).Single();

            Assert.NotNull(found);
            Assert.Equal(DateTimeOffset.MinValue, found.TimeOfPenalty);
            Assert.Equal(PenaltyType.Unreliable, found.Offense);
        }
    }
}
