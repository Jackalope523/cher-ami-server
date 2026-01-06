using CherAmiAPI.Contexts;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Stripe;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Stripe
{
    public class CheckPaymentMethodsEndpoint(ApplicationDbContext ctx, CustomerPaymentMethodService customerPaymentMethodService) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/stripe/payment-methods/check");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            string stripeCustomerId = await ctx.Users
                                      .Where(x => x.Id == userId)
                                      .Select(x => x.StripeCustomerId)
                                      .SingleAsync(cancellationToken: cancellationToken);

            List<PaymentMethod> paymentMethods = (await customerPaymentMethodService.ListAsync(stripeCustomerId, cancellationToken: cancellationToken)).Data;

            await Send.OkAsync(paymentMethods.Count != 0, cancellationToken);
        }
    }
}