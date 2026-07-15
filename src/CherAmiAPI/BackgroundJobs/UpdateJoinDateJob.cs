using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
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
    public class UpdateJoinDateJob(IServiceProvider _serviceProvider) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            ApplicationDbContext ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Log.Error("Starting TimeOfUserAgreement sync job.");
            
            List<User> users = await ctx.Users
                .IgnoreQueryFilters()
                .ToListAsync();
                
            Log.Error($"Syncing TimeOfUserAgreement with JoinDate for {users.Count} users.");

            foreach (var user in users)
            {
                user.TimeOfUserAgreement = user.JoinDate;
            }

            if (users.Count > 0)
            {
                await ctx.SaveChangesAsync();
                Log.Error($"Successfully synced {users.Count} users.");
            }

            Log.Error("Done running TimeOfUserAgreement sync job.");
        }
    }
}
