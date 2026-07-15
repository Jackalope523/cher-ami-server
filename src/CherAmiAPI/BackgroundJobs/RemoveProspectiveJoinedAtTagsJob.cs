using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CherAmiAPI.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public class RemoveProspectiveJoinedAtTagsJob(IServiceProvider _serviceProvider) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            ApplicationDbContext ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            OneSignalService oneSignalService = scope.ServiceProvider.GetRequiredService<OneSignalService>();

            Log.Information("Starting RemoveProspectiveJoinedAtTagsJob: Removing 'joined_at' tags from prospective users.");

            try
            {
                var users = await ctx.Users
                    .AsNoTracking()
                    .Where(u => u.AccountStatus == UserAccountStatus.Prospective)
                    .Select(u => new { u.ExternalId, u.Email })
                    .ToListAsync();

                Log.Information("Found {Count} prospective users to process.", users.Count);

                int successCount = 0;
                int failureCount = 0;

                foreach (var user in users)
                {
                    try
                    {
                        await oneSignalService.RemoveTagAsync(user.ExternalId, "joined_at");
                        successCount++;
                        
                        if (successCount % 10 == 0)
                        {
                            Log.Information("Successfully processed {Count}/{Total} users.", successCount, users.Count);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to remove tag for user {Email} ({ExternalId})", user.Email, user.ExternalId);
                        failureCount++;
                    }
                }

                Log.Information("RemoveProspectiveJoinedAtTagsJob completed. Success: {SuccessCount}, Failure: {FailureCount}", successCount, failureCount);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "RemoveProspectiveJoinedAtTagsJob failed unexpectedly.");
            }
        }
    }
}
