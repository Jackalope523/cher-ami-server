using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.BackgroundJobs
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
                Issue toAdd = new()
                {
                    CircleId = circle.Id,
                    Title = "Issue 1",
                    IssueNumber = 0,
                    DraftingStart = DateTimeOffset.UtcNow,
                    DraftingEnd = DateTimeOffset.UtcNow.AddMonths(1),
                    Status = IssueStatus.Drafting,

                };

                ctx.Issues.Add(toAdd);
            }

            await ctx.SaveChangesAsync();
        }
    }
}
