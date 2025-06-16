using Core.Boundaries;

namespace Repository
{
    public class DisciplineStoreCoordinator : IDisciplineDatabase
    {
        private readonly IDisciplineDatabase store;

        public DisciplineStoreCoordinator(Harbor.Flag flag)
        {
            store = new EFCoreDisciplineStore(flag);
        }

        public async Task<List<Core.Boundaries.GatheringReport>> GetReportsForGatheringAsync(long id)
        {
            return await store.GetReportsForGatheringAsync(id);
        }

        public async Task ReportGatheringAsync(long userId, long gatheringId, DateTimeOffset timeOfReport, GatheringReportType reportType, string reportDetails)
        {
            await store.ReportGatheringAsync(userId, gatheringId, timeOfReport, reportType, reportDetails);
        }

        public async Task ReportUserAsync(long userId, long targetUserId, DateTimeOffset timeOfReport, UserReportType reportType, string reportDetails)
        {
            await store.ReportUserAsync(userId, targetUserId, timeOfReport, reportType, reportDetails);
        }

        public async Task ReportUserAsync(long selfId, long targetId, long gatheringId, DateTimeOffset timeOfReport, UserReportType reportType, string reportDetails)
        {
            await store.ReportUserAsync(selfId, targetId, timeOfReport, reportType, reportDetails);
        }

        public async Task PenaliseUserAsync(long userId, PenaltyType offense, DateTimeOffset timeOfPenalty)
        {
            await store.PenaliseUserAsync(userId, offense, timeOfPenalty);
        }

        public async Task<List<PenaltyShard>> GetPenaltiesForUserAsync(long userId)
        {
            return await store.GetPenaltiesForUserAsync(userId);
        }

        public Task<(List<Core.Boundaries.UserReport>, List<Core.Boundaries.GatheringReport>, List<Core.Boundaries.SnapshotReport>)> GetReportsForUserAsync(long userId)
        {
            return store.GetReportsForUserAsync(userId);
        }

        public Task<(List<Core.Boundaries.UserReport>, List<Core.Boundaries.GatheringReport>, List<Core.Boundaries.SnapshotReport>)> GetReportsByUserAsync(long userId)
        {
            return store.GetReportsByUserAsync(userId);
        }

        public Task<List<Core.Boundaries.SnapshotReport>> GetReportsForSnapshotAsync(long snapshotId)
        {
            return store.GetReportsForSnapshotAsync(snapshotId);
        }

        public Task ReportSnapshotAsync(long userId, long snapshotId, DateTimeOffset timeOfReport, SnapshotReportType reportType, string reportDetails)
        {
            return store.ReportSnapshotAsync(userId, snapshotId, timeOfReport, reportType, reportDetails);
        }
    }
}
