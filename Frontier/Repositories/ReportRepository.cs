using CrazyLizard.Contexts;
using Microsoft.EntityFrameworkCore;
using CrazyLizard.Entities.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PostReport = CrazyLizard.Entities.Reports.PostReport;
using UserReport = CrazyLizard.Entities.Reports.UserReport;
using CrazyLizard.Interfaces.Repository;


namespace CrazyLizard.Repositories
{
    public class ReportRepository(ApplicationDbContext ctx) : IReportRepository
    {

        public async Task ReportUserAsync(long userId, long targetUserId, DateTimeOffset timeOfReport, UserReportType reportType, string reportDetails)
        {
            UserReport toCreate = new()
            {
                FilingUserId = userId,
                ReportedUserId = targetUserId,
                Type = reportType,
                FilingDate = timeOfReport,
                Notes = reportDetails
            };

            ctx.UserReports.Add(toCreate);
            await ctx.SaveChangesAsync();
        }
        public async Task<(List<UserReport>, List<PostReport>)> GetReportsForUserAsync(long userId)
        {
            List<UserReport> userReports = await ctx.UserReports.Where(r => r.ReportedUserId == userId). ToListAsync();

            List<PostReport> postReports = await ctx.Posts.
                                                           Where(p => p.AuthorId == userId).
                                                           Join
                                                           (
                                                                ctx.PostReports,
                                                                p => p.Id,
                                                                pr => pr.PostId,
                                                                (p, pr) => pr).
                                                           ToListAsync();

            return (userReports, postReports);
        }

        public async Task<(List<UserReport>, List<PostReport>)> GetReportsByUserAsync(long userId)
        {
            List<Report> reports = await ctx.Reports.Where(r => r.FilingUserId == userId).ToListAsync();

            List<UserReport> userReports = reports.OfType<UserReport>().ToList();
            List<PostReport> postReports = reports.OfType<PostReport>().ToList();

            return (userReports, postReports);
        }

        public async Task<List<PostReport>> GetReportsForPostAsync(long postId)
        {
            return await ctx.PostReports.Where(r => r.PostId == postId).ToListAsync();
        }

        public async Task ReportPostAsync(long userId, long snapshotId, DateTimeOffset timeOfReport, PostReportType reportType, string reportDetails)
        {
            PostReport toCreate = new()
            {
                FilingUserId = userId,
                PostId = snapshotId,
                Type = reportType,
                FilingDate = timeOfReport,
                Notes = reportDetails
            };

            ctx.PostReports.Add(toCreate);
            await ctx.SaveChangesAsync();
        }
    }
}
