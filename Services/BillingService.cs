using CherAmiAPI.Entities;
using CherAmiAPI.Exceptions;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Shared.Responses;
using Microsoft.Extensions.Configuration;
using Stripe;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Services
{
    public class BillingService(
        IUserRepository userRepository,
        IRecipientRepository recipientRepository,
        IConfiguration config,
        SetupIntentService setupIntentService,
        CustomerPaymentMethodService customerPaymentMethodService,
        PaymentMethodService paymentMethodService,
        PriceService priceService)
    {
        public async Task<SetupIntentResponse> CreateSetupIntentAsync(long userId, CancellationToken cancellationToken = default)
        {
            User user = await userRepository.GetUserAsync(userId, cancellationToken);

            return CreateSetupIntent(user.StripeCustomerId);
        }

        public async Task<List<PaymentMethod>> GetPaymentMethodsAsync(long userId, CancellationToken cancellationToken = default)
        {
            User user = await userRepository.GetUserAsync(userId, cancellationToken);

            return (await customerPaymentMethodService.ListAsync(user.StripeCustomerId, cancellationToken: cancellationToken)).Data;
        }

        public async Task RemovePaymentMethodAsync(long userId, CancellationToken cancellationToken = default)
        {
            User user = await userRepository.GetUserAsync(userId, cancellationToken);
            int recipientCount = await recipientRepository.CountRecipientsOfManagerAsync(userId, cancellationToken);

            if (recipientCount != 0)
                throw new ConflictException($"User {userId} still has {recipientCount} recipients.");

            List<PaymentMethod> paymentMethods = (await customerPaymentMethodService.ListAsync(user.StripeCustomerId, cancellationToken: cancellationToken)).Data;

            await paymentMethodService.DetachAsync(paymentMethods[0].Id, cancellationToken: cancellationToken);
        }

        public async Task<SetupIntentResponse> ReplacePaymentMethodAsync(long userId, CancellationToken cancellationToken = default)
        {
            User user = await userRepository.GetUserAsync(userId, cancellationToken);

            PaymentMethod paymentMethod = (await customerPaymentMethodService.ListAsync(user.StripeCustomerId, cancellationToken: cancellationToken)).Data.Single();
            await paymentMethodService.DetachAsync(paymentMethod.Id, cancellationToken: cancellationToken);

            return CreateSetupIntent(user.StripeCustomerId);
        }

        public async Task<long?> GetStandardPriceAsync(long userId, CancellationToken cancellationToken = default)
        {
            User user = await userRepository.GetUserAsync(userId, cancellationToken);

            if (user.IsBillingExempt)
                return 0L;

            Price price = await priceService.GetAsync(config["MONTHLY_MAGAZINE_STANDARD_PRICE_ID"], cancellationToken: cancellationToken);

            return price.UnitAmount;
        }

        public async Task<(long StandardEditionPrice, long MilitaryEditionPrice)> GetPricesAsync(long userId, CancellationToken cancellationToken = default)
        {
            User user = await userRepository.GetUserAsync(userId, cancellationToken);

            if (user.IsBillingExempt)
                return (0L, 0L);

            Price standardPrice = await priceService.GetAsync(config["MONTHLY_MAGAZINE_STANDARD_PRICE_ID"], cancellationToken: cancellationToken);
            Price militaryPrice = await priceService.GetAsync(config["MONTHLY_MAGAZINE_MILITARY_PRICE_ID"], cancellationToken: cancellationToken);

            return ((long)standardPrice.UnitAmount, (long)militaryPrice.UnitAmount);
        }

        private SetupIntentResponse CreateSetupIntent(string stripeCustomerId)
        {
            SetupIntentCreateOptions options = new()
            {
                Customer = stripeCustomerId,
            };

            SetupIntent setupIntent = setupIntentService.Create(options);

            return new SetupIntentResponse
            {
                ClientSecret = setupIntent.ClientSecret,
                CustomerId = stripeCustomerId,
                MerchantDisplayName = "Cher Ami",
                AllowsDelayedPaymentMethods = false,
            };
        }
    }
}
