using CherAmiAPI.Contexts;
using CherAmiAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CherAmiAPI.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public class AddMarketingTagsJob(IServiceProvider _serviceProvider) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            ApplicationDbContext ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            OneSignalService oneSignalService = scope.ServiceProvider.GetRequiredService<OneSignalService>();

            Log.Error("Starting AddMarketingTagsJob: Adding 'email_reminders' and 'email_marketing' tags to existing users.");

            try
            {
                // Get all users who have a OneSignalId
                var users = await ctx.Users
                    .AsNoTracking()
                    .Where(u => u.OneSignalId != default)
                    .Select(u => new { u.ExternalId, u.Email })
                    .ToListAsync();

                Log.Error("Found {Count} users to update.", users.Count);

                int successCount = 0;
                int failureCount = 0;

                foreach (var user in users)
                {
                    try
                    {
                        await oneSignalService.AddTagAsync(user.ExternalId, "email_reminders", "1");
                        await oneSignalService.AddTagAsync(user.ExternalId, "email_marketing", "1");
                        successCount++;
                        
                        if (successCount % 10 == 0)
                        {
                            Log.Error("Successfully updated {Count}/{Total} users.", successCount, users.Count);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to update tags for user {Email} ({ExternalId})", user.Email, user.ExternalId);
                        failureCount++;
                    }
                }

                Log.Error("AddMarketingTagsJob completed. Success: {SuccessCount}, Failure: {FailureCount}", successCount, failureCount);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "AddMarketingTagsJob failed unexpectedly.");
            }
        }
    }
}
