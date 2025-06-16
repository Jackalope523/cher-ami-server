using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Boundaries
{
	#region Schemas

    public enum PenaltyType
    { Unreliable }

    public enum UserReportType
    {
        Rude, HateSpeech, Harassment,
        ViolenceOrAssault, Other
    }

    public enum GatheringReportType
    {
        InappropriateGathering, InappropriateHeader, Misleading,
        Illegal, Promotion, Spam, Other
    }

    public enum SnapshotReportType
    {
        Inappropriate, GraphicContent, ManipulatedMedia,
        Promotion, Spam, Other
    }

    public record PenaltyShard(PenaltyType Offense, DateTimeOffset TimeOfPenalty);

	public record UserReport(long Id, long ReportingUserId, long ReportedUserId, DateTimeOffset ReportTime,
        UserReportType ReportType, string ReportDetails);

    public record GatheringReport(long Id, long ReportingUserId, long ReportedGatheringId, DateTimeOffset ReportTime,
        GatheringReportType ReportType, string ReportDetails);

    public record SnapshotReport(long Id, long ReportingUserId, long ReportedSnapshotId, DateTimeOffset ReportTime,
        SnapshotReportType ReportType, string ReportDetails);

	#endregion

	#region Gates

	public interface IDisciplineDatabase
    {
        Task<List<PenaltyShard>> GetPenaltiesForUserAsync(long userId);
        Task PenaliseUserAsync(long userId, PenaltyType offense, DateTimeOffset timeOfPenalty);

        Task<(List<UserReport>, List<GatheringReport>, List<SnapshotReport>)> GetReportsForUserAsync(long userId);
        Task<(List<UserReport>, List<GatheringReport>, List<SnapshotReport>)> GetReportsByUserAsync(long userId);
        Task ReportUserAsync(long userId, long targetUserId, long gatheringId, DateTimeOffset timeOfReport,
            UserReportType reportType, string reportDetails);
        Task ReportUserAsync(long userId, long targetUserId, DateTimeOffset timeOfReport,
            UserReportType reportType, string reportDetails);

        Task<List<GatheringReport>> GetReportsForGatheringAsync(long gatheringId);
        Task ReportGatheringAsync(long userId, long gatheringId, DateTimeOffset timeOfReport,
            GatheringReportType reportType, string reportDetails);

        Task<List<SnapshotReport>> GetReportsForSnapshotAsync(long snapshotId);
        Task ReportSnapshotAsync(long userId, long snapshotId, DateTimeOffset timeOfReport,
            SnapshotReportType reportType, string reportDetails);
    }

    public interface IDisciplineOperations
    {
        Task<List<UserReportType>> GetAvailableReportsForUserAsync(long userId, long targetId);
        Task ReportUserAsync(long userId, long targetId,
            UserReportType reportType, string reportDetails,
            long? gatheringId = null);

        Task<List<GatheringReportType>> GetAvailableReportsForGatheringAsync(long userId, long gatheringId);
        Task ReportGatheringAsync(long userId, long gatheringId,
            GatheringReportType reportType, string reportDetails);

        Task<List<SnapshotReportType>> GetAvailableReportsForSnapshotAsync(long userId, long snapshotId);
        Task ReportSnapshotAsync(long userId, long snapshotId,
            SnapshotReportType reportType, string reportDetails);
    }

	#endregion
}

