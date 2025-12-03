using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
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
            CustomerPaymentMethodService customerPaymentMethodService = scope.ServiceProvider.GetRequiredService<CustomerPaymentMethodService>();

            var data = await ctx.Users
                        .Select(x => new { User = x, RecipientsCount = x.Recipients.Count })
                        .Where(x => x.User.StripeCustomerId != null)
                        .ToListAsync();

            foreach (var entry in data)
            {
                bool hasSubscription = !string.IsNullOrWhiteSpace(entry.User.StripeSubscriptionId);
                bool hasRecipients = entry.RecipientsCount > 0;

                if (hasRecipients && hasSubscription)
                {
                    Subscription subscription = await subscriptionService.GetAsync(entry.User.StripeSubscriptionId);

                    SubscriptionItem subscriptionItem = subscription.Items.First();

                    if (subscriptionItem.Quantity != entry.RecipientsCount)
                    {
                        SubscriptionItemUpdateOptions subscriptionItemOptions = new()
                        {
                            Quantity = entry.RecipientsCount,
                            ProrationBehavior = "none",
                        };

                        await subscriptionItemService.UpdateAsync(subscriptionItem.Id, subscriptionItemOptions);
                    }
                }
                else if (hasRecipients && !hasSubscription)
                {
                    SubscriptionCreateOptions subscriptionOptions = new()
                    {
                        Customer = entry.User.StripeCustomerId,
                        Items =
                        [
                           new()
                           {
                               Price = "price_1S7govARYKi6NXMeuiOwG70F",
                               Quantity = entry.RecipientsCount,
                           },
                        ],
                        PaymentSettings = new() { SaveDefaultPaymentMethod = "on_subscription" },
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

                    Subscription subscription = await subscriptionService.CreateAsync(subscriptionOptions);
                    entry.User.StripeSubscriptionId = subscription.Id;
                }
                else if (!hasRecipients && hasSubscription)
                {
                    await subscriptionService.CancelAsync(entry.User.StripeSubscriptionId);
                    entry.User.StripeSubscriptionId = null;
                }
                else
                {
                    continue;
                }
            }

            await ctx.SaveChangesAsync();
        }
    }
}
