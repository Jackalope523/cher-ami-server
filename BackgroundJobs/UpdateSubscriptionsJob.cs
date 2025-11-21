using CherAmiAPI.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Stripe;
using System;
using System.Linq;
using System.Threading.Tasks;
using Subscription = Stripe.Subscription;

namespace CherAmiAPI.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public class UpdateSubscriptionsJob(IServiceProvider _serviceProvider) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            ApplicationDbContext ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            SubscriptionService subscriptionService = scope.ServiceProvider.GetRequiredService<SubscriptionService>();
            SubscriptionItemService subscriptionItemService = scope.ServiceProvider.GetRequiredService<SubscriptionItemService>();

            var users = await ctx.Users
                        .Select(x => new { x.Id, x.StripeCustomerId, x.StripeSubscriptionId, x.Recipients.Count})
                        .Where(x => x.StripeCustomerId != null)
                        .ToListAsync();

            foreach (var user in users)
            {
                if (!string.IsNullOrWhiteSpace(user.StripeSubscriptionId))
                {
                    Subscription subscription = await subscriptionService.GetAsync(user.StripeSubscriptionId);

                    SubscriptionItem subscriptionItem = subscription.Items.First();

                    SubscriptionItemUpdateOptions subscriptionItemOptions = new()
                    {
                        Quantity = user.Count,
                        ProrationBehavior = "none",
                    };

                    await subscriptionItemService.UpdateAsync(subscriptionItem.Id, subscriptionItemOptions);
                }
                else
                {
                    SubscriptionCreateOptions subscriptionOptions = new()
                    {
                        Customer = user.StripeCustomerId,
                        Items =
                        [
                            new()
                        {
                            Price = "prod_T3oReCcNZqq7wm",
                            Quantity = user.Count,
                        },
                    ],
                        PaymentSettings = { SaveDefaultPaymentMethod = "on_subscription" },
                        PaymentBehavior = "default_incomplete",
                        BillingMode = new() { Type = "flexible" },
                        ProrationBehavior = "none",
                        BillingCycleAnchorConfig = new SubscriptionBillingCycleAnchorConfigOptions()
                        {
                            DayOfMonth = 1,
                            Hour = 4, 
                            Minute = 0, 
                            Second = 0,
                        }
                    };

                    await subscriptionService.CreateAsync(subscriptionOptions);
                }

            }

            await ctx.SaveChangesAsync();
        }
    }
}
