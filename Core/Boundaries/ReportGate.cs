using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Boundaries
{
	#region Schemas

    public enum UserReportType
    {
        Rude, HateSpeech,
        Harassment, Other
    }

    public enum PostReportType
    {
        Embarrassing, Inappropriate,
        GraphicContent, ManipulatedMedia,
        Spam, Other
    }

	public record UserReport(long Id, long ReportingUserId, long ReportedUserId, DateTimeOffset ReportTime,
        UserReportType ReportType, string ReportDetails);

    public record PostReport(long Id, long ReportingUserId, long ReportedSnapshotId, DateTimeOffset ReportTime,
        PostReportType ReportType, string ReportDetails);

	#endregion

	#region Gates

	public interface IReportDatabase
    {
        Task<(List<UserReport>, List<PostReport>)> GetReportsForUserAsync(long userId);
        Task<(List<UserReport>, List<PostReport>)> GetReportsByUserAsync(long userId);

        Task ReportUserAsync(long userId, long targetUserId, DateTimeOffset timeOfReport,
            UserReportType reportType, string reportDetails);

        Task<List<PostReport>> GetReportsForPostAsync(long snapshotId);
        Task ReportSnapshotAsync(long userId, long snapshotId, DateTimeOffset timeOfReport,
            PostReportType reportType, string reportDetails);
    }

    public interface IReportOperations
    {
        Task<List<UserReportType>> GetAvailableReportsForUserAsync(long userId, long targetId);
        Task ReportUserAsync(long userId, long targetId,
            UserReportType reportType, string reportDetails,
            long? circleId = null);

        Task<List<PostReportType>> GetAvailableReportsForPostAsync(long userId, long postId);
        Task ReportPostAsync(long userId, long postId,
            PostReportType reportType, string reportDetails);
    }

	#endregion
}

