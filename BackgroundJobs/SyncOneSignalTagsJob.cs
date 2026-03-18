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
    public class SyncOneSignalTagsJob(IServiceProvider _serviceProvider) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            ApplicationDbContext ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            OneSignalService oneSignalService = scope.ServiceProvider.GetRequiredService<OneSignalService>();

            Log.Information("Starting SyncOneSignalTagsJob: Adding 'joined_at' tags to existing users.");

            try
            {
                // Get all users who have a JoinDate
                var users = await ctx.Users
                    .AsNoTracking()
                    .Where(u => u.OneSignalId != default)
                    .Select(u => new { u.ExternalId, u.JoinDate, u.Email })
                    .ToListAsync();

                Log.Information("Found {Count} users to sync.", users.Count);

                int successCount = 0;
                int failureCount = 0;

                foreach (var user in users)
                {
                    try
                    {
                        string unixTimestamp;
                        if (user.JoinDate == default)
                        {
                            unixTimestamp = DateTimeOffset.UtcNow.AddMonths(-2).ToUnixTimeSeconds().ToString();
                        }
                        else
                        {
                            unixTimestamp = user.JoinDate.ToUnixTimeSeconds().ToString();
                        }
                        await oneSignalService.AddTagAsync(user.ExternalId, "joined_at", unixTimestamp);
                        successCount++;
                        
                        if (successCount % 10 == 0)
                        {
                            Log.Information("Successfully synced {Count}/{Total} users.", successCount, users.Count);
                        }
                    }

                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to sync tag for user {Email} ({ExternalId})", user.Email, user.ExternalId);
                        failureCount++;
                    }
                }

                Log.Information("SyncOneSignalTagsJob completed. Success: {SuccessCount}, Failure: {FailureCount}", successCount, failureCount);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "SyncOneSignalTagsJob failed unexpectedly.");
            }
        }
    }
}
