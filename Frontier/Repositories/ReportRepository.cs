using Microsoft.EntityFrameworkCore;
using Repository.Contexts;
using Repository.Entities.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserReport = Repository.Entities.Reports.UserReport;


namespace Repository.Repositories
{
    public class ReportRepository(LLContext ctx) : IReportRepository
    {

        public async Task ReportUserAsync(long userId, long targetUserId, DateTimeOffset timeOfReport, UserReportType reportType, string reportDetails)
        {
            UserReport toCreate = new()
            {
                FilingUserId = userId,
                UserId = targetUserId,
                Type = reportType,
                FilingDate = timeOfReport,
                Notes = reportDetails
            };

            ctx.UserReports.Add(toCreate);
            await ctx.SaveChangesAsync();
        }
        public async Task<(List<Core.Boundaries.UserReport>, List<Core.Boundaries.PostReport>)> GetReportsForUserAsync(long userId)
        {
            List<Core.Boundaries.UserReport> userReports = await ctx.UserReports.
                                                           Where(r => r.UserId == userId).
                                                           Select(r => new Core.Boundaries.UserReport
                                                           (
                                                               r.Id, 
                                                               r.FilingUserId ?? 0, 
                                                               r.UserId, 
                                                               r.FilingDate, 
                                                               r.Type, 
                                                               r.Notes
                                                           )).
                                                           ToListAsync();

            List<Core.Boundaries.PostReport> postReports = await ctx.Posts.
                                                           Where(p => p.AuthorId == userId).
                                                           Join
                                                           (
                                                                ctx.PostReports,
                                                                p => p.Id,
                                                                pr => pr.PostId,
                                                                (p, pr) => new Core.Boundaries.PostReport
                                                           (
                                                               pr.Id,
                                                               pr.FilingUserId ?? 0,
                                                               pr.PostId,
                                                               pr.FilingDate,
                                                               pr.Type,
                                                               pr.Notes
                                                           )).
                                                           ToListAsync();

            return (userReports, postReports);
        }

        public async Task<(List<Core.Boundaries.UserReport>, List<Core.Boundaries.PostReport>)> GetReportsByUserAsync(long userId)
        {
            List<Report> reports = await ctx.Reports.Where(r => r.FilingUserId == userId).ToListAsync();

            List<UserReport> userReports = reports.OfType<UserReport>().ToList();
            List<Core.Boundaries.PostReport> postReports = reports.OfType<Core.Boundaries.PostReport>().ToList();

            return (userReports, reports.OfType<Core.Boundaries.PostReport>().ToList());
        }

        public async Task<List<Core.Boundaries.PostReport>> GetReportsForPostAsync(long postId)
        {
            return await 
            ctx.PostReports.
            Where(r => r.PostId == postId).
            Select(r => new Core.Boundaries.PostReport
            (
                r.Id,
                r.FilingUserId ?? 0,
                r.PostId,
                r.FilingDate,
                r.Type,
                r.Notes
            )).
            ToListAsync();
        }

        public async Task ReportPostAsync(long userId, long snapshotId, DateTimeOffset timeOfReport, PostReportType reportType, string reportDetails)
        {
            Entities.Reports.PostReport toCreate = new()
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
