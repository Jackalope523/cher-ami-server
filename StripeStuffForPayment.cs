using Stripe;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Subscription = Stripe.Subscription;

namespace CherAmiAPI
{
    public class StripeStuffForPayment()
    {
        //public async Task<string> CreateSetupIntentAsync(long userId, CancellationToken cancellationToken = default)
        //{
        //    User user = await accountRepository.GetUserByIdAsync(userId);

        //    Customer customer;
        //    if (!string.IsNullOrEmpty(user.StripeCustomerId))
        //    {
        //        customer = await stripeClient.V1.Customers.GetAsync(user.StripeCustomerId, cancellationToken: cancellationToken);
        //    }
        //    else
        //    {
        //        CustomerCreateOptions customerOptions = new()
        //        {
        //            Name = $"{user.Title} {user.FirstName} {user.LastName}",
        //            Phone = user.PhoneNumber,
        //        };

        //        customer = await stripeClient.V1.Customers.CreateAsync(customerOptions, cancellationToken: cancellationToken);
        //        await accountRepository.UpdateStripeCustomerIdAsync(user.Id, customer.Id);
        //    }

        //    SetupIntentCreateOptions options = new()
        //    {
        //        Customer = customer.Id,
        //    };

        //    SetupIntent setupIntent = await stripeClient.V1.SetupIntents.CreateAsync(options, cancellationToken: cancellationToken);

        //    return setupIntent.ClientSecret;
        //}

  

       
        //public async Task RemoveRecipientAsync(long userId, long recipientId, CancellationToken cancellationToken = default)
        //{
        //    if (!await circleRepository.HasCircle(userId))
        //        throw new NotFoundException($"User {userId} does not have a circle.");

        //    if (!await accountRepository.IsManagerAsync(userId, recipientId))
        //        throw new NoAccessException($"User {userId} does not manage recipient {recipientId}.");

        //    User user = await accountRepository.GetUserByIdAsync(userId);

        //    if (!user.ProvidedPaymentDetails)
        //        throw new NotFoundException($"User {userId} has not provided payment details.");

        //    if (string.IsNullOrWhiteSpace(user.StripeSubscriptionId))
        //        throw new NotFoundException($"User {userId} does not have a subscription.");

        //    Subscription subscription = await stripeClient.V1.Subscriptions.GetAsync(user.StripeSubscriptionId, cancellationToken: cancellationToken);

        //    if (subscription.Items.Count() > 1)
        //    {
        //        SubscriptionItem subscriptionItem = subscription.Items.First();

        //        SubscriptionItemUpdateOptions subscriptionItemOptions = new()
        //        {
        //            Quantity = subscriptionItem.Quantity - 1,
        //            ProrationBehavior = "none",
        //        };

        //        await stripeClient.V1.SubscriptionItems.UpdateAsync(subscriptionItem.Id, subscriptionItemOptions, cancellationToken: cancellationToken);
        //    }
        //    else
        //    {
        //        SubscriptionCancelOptions subscriptionOptions = new()
        //        {
        //            InvoiceNow = false,
        //            Prorate = false
        //        };

        //        await stripeClient.V1.Subscriptions.CancelAsync(subscription.Id, subscriptionOptions, cancellationToken: cancellationToken);
        //    }

        //    await accountRepository.RemoveRecipientAsync(recipientId);
        //}

        //public async Task AddRecipientAsync(long userId, Recipient recipient, CancellationToken cancellationToken = default)
        //{
        //    if (!await circleRepository.HasCircle(userId))
        //        throw new NotFoundException($"User {userId} does not have a circle.");

        //    User user = await accountRepository.GetUserByIdAsync(userId);

        //    if (!user.ProvidedPaymentDetails)
        //        throw new NotFoundException($"User {userId} has not provided payment details.");
         
        //    if (!string.IsNullOrWhiteSpace(user.StripeSubscriptionId))
        //    {
        //        Subscription subscription = await stripeClient.V1.Subscriptions.GetAsync(user.StripeSubscriptionId, cancellationToken: cancellationToken);

        //        SubscriptionItem subscriptionItem = subscription.Items.First();

        //        SubscriptionItemUpdateOptions subscriptionItemOptions = new()
        //        {
        //            Quantity = subscriptionItem.Quantity + 1,
        //            ProrationBehavior = "none",
        //        };

        //        await stripeClient.V1.SubscriptionItems.UpdateAsync(subscriptionItem.Id, subscriptionItemOptions, cancellationToken: cancellationToken);
        //    }
        //    else
        //    {
        //        SubscriptionCreateOptions subscriptionOptions = new()
        //        {
        //            Customer = user.StripeCustomerId,
        //            Items =
        //            [
        //                new()
        //                {
        //                    Price = "prod_T3oReCcNZqq7wm",
        //                    Quantity = 1,
        //                },
        //            ],
        //            PaymentSettings =  { SaveDefaultPaymentMethod = "on_subscription" },
        //            PaymentBehavior = "default_incomplete",
        //            BillingMode = new() { Type = "flexible" },
        //        };

        //        await stripeClient.V1.Subscriptions.CreateAsync(subscriptionOptions, cancellationToken: cancellationToken);
        //    }

        //    await accountRepository.AddRecipientAsync(recipient);
        //}
    }
}
