using CrazyLizard.Entities.Reports;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrazyLizard.Interfaces.Repository
{
    public interface IReportRepository
    {
        Task<(List<UserReport>, List<PostReport>)> GetReportsForUserAsync(long userId);
        Task<(List<UserReport>, List<PostReport>)> GetReportsByUserAsync(long userId);
        Task ReportUserAsync(long userId, long targetUserId, DateTimeOffset timeOfReport, UserReportType reportType, string reportDetails);

        Task<List<PostReport>> GetReportsForPostAsync(long snapshotId);
        Task ReportPostAsync(long userId, long snapshotId, DateTimeOffset timeOfReport, PostReportType reportType, string reportDetails);
    }
}

