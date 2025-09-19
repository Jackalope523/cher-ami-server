using CrazyLizard.Entities.Reports;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrazyLizard.Interfaces.Service
{
    public interface IReportService
    {
        Task<List<UserReportType>> GetAvailableReportsForUserAsync(long userId, long targetId);
        Task ReportUserAsync(long userId, long targetId, UserReportType reportType, string reportDetails, long? circleId = null);

        Task<List<PostReportType>> GetAvailableReportsForPostAsync(long userId, long postId);
        Task ReportPostAsync(long userId, long postId, PostReportType reportType, string reportDetails);
    }
}

