using CherAmiAPI.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using Stripe;
using System;
using System.Linq;
using System.Threading.Tasks;
using Subscription = Stripe.Subscription;

namespace CherAmiAPI.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public class KeepAliveJob(IServiceProvider _serviceProvider) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            ApplicationDbContext ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Log.Error("Starting keep alive job");
            while (true)
            {
                try
                {
                    await ctx.Users.AnyAsync();
                    Log.Error("Database awake.");
                    return;
                }
                catch (Exception)
                {
                    Log.Error("Database asleep waiting 2 minutes...");
                    await Task.Delay(TimeSpan.FromMinutes(2));
                }
            }
        }
    }
}
