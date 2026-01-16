using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CherAmiAPI.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public class MonthlyIssueJob(IServiceProvider _serviceProvider) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            ApplicationDbContext ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            List<Circle> circles = await ctx.Circles.ToListAsync();

            foreach (Circle circle in circles)
            {
                int lastIssueNumber = await ctx.Issues
                                      .Where(i => i.CircleId == circle.Id)
                                      .Select(i => i.IssueNumber)
                                      .OrderByDescending(i => i)
                                      .FirstOrDefaultAsync();

                Issue toAdd = new()
                {
                    CircleId = circle.Id,
                    Title = $"Issue {lastIssueNumber + 1} · {DateTime.UtcNow:MMMM yyyy}",
                    IssueNumber = lastIssueNumber + 1,
                    DraftingStart = new DateTimeOffset(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1), TimeSpan.Zero),
                    DraftingEnd = new DateTimeOffset(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).AddTicks(-1), TimeSpan.Zero),
                    Status = IssueStatus.Drafting,
                };

                ctx.Issues.Add(toAdd);
            }

            await ctx.SaveChangesAsync();
        }
    }
}
