using CherAmiAPI.Contexts;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Stripe;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.PaymentMethods
{
    public class UpdatePaymentMethodEndpoint(ApplicationDbContext ctx, SetupIntentService setupIntentService, CustomerPaymentMethodService customerPaymentMethodService, PaymentMethodService paymentMethodService) : EndpointWithoutRequest<SetupIntentResponse>
    {
        public override void Configure()
        {
            Patch("/payment-method");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            string stripeCustomerId = await ctx.Users.Where(x => x.Id == userId).Select(x => x.StripeCustomerId).SingleAsync(cancellationToken: cancellationToken);

            PaymentMethod paymentMethod = (await customerPaymentMethodService.ListAsync(stripeCustomerId, cancellationToken: cancellationToken)).Data.Single();
            await paymentMethodService.DetachAsync(paymentMethod.Id, cancellationToken: cancellationToken);

            SetupIntentCreateOptions options = new()
            {
                Customer = stripeCustomerId,
            };

            SetupIntent setupIntent = setupIntentService.Create(options);

            SetupIntentResponse response = new()
            {
                ClientSecret = setupIntent.ClientSecret,
                CustomerId = stripeCustomerId,
                MerchantDisplayName = "Cher Ami",
                AllowsDelayedPaymentMethods = false,
            };

            await Send.OkAsync(response, cancellationToken);
        }
    }
}