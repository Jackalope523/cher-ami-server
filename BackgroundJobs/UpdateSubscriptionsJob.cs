using CherAmiAPI.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using SQLitePCL;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Subscription = Stripe.Subscription;

namespace CherAmiAPI.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public class UpdateSubscriptionsJob(IConfiguration config, IServiceProvider _serviceProvider) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            ApplicationDbContext ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            SubscriptionService subscriptionService = scope.ServiceProvider.GetRequiredService<SubscriptionService>();
            SubscriptionItemService subscriptionItemService = scope.ServiceProvider.GetRequiredService<SubscriptionItemService>();
            CustomerPaymentMethodService customerPaymentMethodService = scope.ServiceProvider.GetRequiredService<CustomerPaymentMethodService>();

            List<string> priceIds = [config["MONTHLY_MAGAZINE_STANDARD_PRICE_ID"], config["MONTHLY_MAGAZINE_MILITARY_PRICE_ID"]];

            var data = await ctx.Users
                    .Select(x => new 
                    { 
                        User = x, 
                        MilitaryRecipientsCount = x.Recipients.Count(x => x.IsVeteran), 
                        StandardRecipientsCount = x.Recipients.Count(x => !x.IsVeteran) 
                    })
                    .ToListAsync();

            foreach (var entry in data)
            {
                bool hasSubscription = !string.IsNullOrWhiteSpace(entry.User.StripeSubscriptionId);
                bool hasRecipients = entry.StandardRecipientsCount > 0 || entry.MilitaryRecipientsCount > 0;
                List<int> itemCounts = [entry.StandardRecipientsCount, entry.MilitaryRecipientsCount];

                if (hasRecipients && hasSubscription)
                {
                    Subscription subscription = await subscriptionService.GetAsync(entry.User.StripeSubscriptionId);
                    Dictionary<string, SubscriptionItem> subscriptionItems = subscription.Items.ToDictionary(i => i.Price.Id, i => i);

                    for (int i = 0; i < priceIds.Count; i++)
                    {
                        bool itemExists = subscriptionItems.TryGetValue(priceIds[i], out SubscriptionItem subscriptionItem);
                        if (itemExists && subscriptionItem.Quantity != itemCounts[i])
                        {
                            if (itemCounts[i] == 0)
                            {
                                SubscriptionItemDeleteOptions subscriptionItemOptions = new()
                                {
                                    ProrationBehavior = "none",
                                };

                                await subscriptionItemService.DeleteAsync(subscriptionItem.Id);
                            }
                            else
                            {
                                SubscriptionItemUpdateOptions subscriptionItemOptions = new()
                                {
                                    Quantity = itemCounts[i],
                                    ProrationBehavior = "none",
                                };

                                await subscriptionItemService.UpdateAsync(subscriptionItem.Id, subscriptionItemOptions);
                            }
                        }
                        if (!itemExists && itemCounts[i] > 0)
                        {
                            SubscriptionItemCreateOptions subscriptionItemOptions = new()
                            {
                                Subscription = subscription.Id,
                                Price = priceIds[i],                 
                                Quantity = itemCounts[i],
                                ProrationBehavior = "none"
                            };

                            await subscriptionItemService.CreateAsync(subscriptionItemOptions);
                        }
                    }
                }
                else if (hasRecipients && !hasSubscription && !entry.User.IsBillingExempt)
                {
                    List<SubscriptionItemOptions> subscriptionItemOptions = [];
                    for (int i = 0; i < priceIds.Count; i++)
                    {
                        if (itemCounts[i] > 0)
                        {
                            SubscriptionItemOptions options = new()
                            {
                                Price = priceIds[i],
                                Quantity = itemCounts[i],
                            };

                            subscriptionItemOptions.Add(options);
                        }
                    }

                    SubscriptionCreateOptions subscriptionOptions = new()
                    {
                        Customer = entry.User.StripeCustomerId,
                        Items = subscriptionItemOptions,
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
                        },
                    };

                    Subscription subscription = await subscriptionService.CreateAsync(subscriptionOptions);
                    entry.User.StripeSubscriptionId = subscription.Id;
                }
                else if (hasSubscription && (!hasRecipients || entry.User.IsBillingExempt))
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
