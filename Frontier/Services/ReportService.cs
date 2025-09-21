using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrazyLizard.Boundaries.Repository;
using CrazyLizard.Entities.Reports;
using CrazyLizard.Interfaces.Repository;
using CrazyLizard.Interfaces.Service;

namespace CrazyLizard.Services
{
    public class ReportService(IReportRepository reportRepository, IAccountRepository accountRepository) : IReportService
	{
        public async Task<List<UserReportType>> GetAvailableReportsForUserAsync(long userId, long targetId)
        {
            throw new NotImplementedException();
        }

        public async Task ReportUserAsync(long userId, long targetId, UserReportType reportType, string reportDetails, long? gatheringId = null)
        {
            throw new NotImplementedException();
        }

        public async Task<List<PostReportType>> GetAvailableReportsForPostAsync(long userId, long postId)
        {
            throw new NotImplementedException();
        }

        public async Task ReportPostAsync(long userId, long snapshotId, PostReportType reportType, string reportDetails)
        {
            throw new NotImplementedException();
        }
	}
}

