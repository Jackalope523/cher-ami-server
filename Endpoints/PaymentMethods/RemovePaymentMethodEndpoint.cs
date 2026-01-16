using CherAmiAPI.Contexts;
using CherAmiAPI.Exceptions;
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
    public class RemovePaymentMethodEndpoint(ApplicationDbContext ctx, CustomerPaymentMethodService customerPaymentMethodService, PaymentMethodService paymentMethodService) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Delete("/payment-method");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var result = await ctx.Users
                            .Where(u => u.Id == userId)
                            .Select(u => new { u.StripeCustomerId, RecipientCount = u.Recipients.Count })
                            .SingleAsync(cancellationToken);

            if (result.RecipientCount != 0)
            {
                throw new ConflictException($"User {userId} still has {result.RecipientCount} recipients.");
            }

            List<PaymentMethod> paymentMethods = (await customerPaymentMethodService.ListAsync(result.StripeCustomerId, cancellationToken: cancellationToken)).Data;

            await paymentMethodService.DetachAsync(paymentMethods[0].Id, cancellationToken: cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}