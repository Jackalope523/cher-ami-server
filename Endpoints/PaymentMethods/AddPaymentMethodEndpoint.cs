using CherAmiAPI.Contexts;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Stripe;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.PaymentMethods
{
    public class AddPaymentMethodEndpoint(ApplicationDbContext ctx, SetupIntentService setupIntentService) : EndpointWithoutRequest<SetupIntentResponse>
    {
        public override void Configure()
        {
            Post("/payment-method");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            string stripeCustomerId = await ctx.Users.Where(x => x.Id == userId).Select(x => x.StripeCustomerId).SingleAsync(cancellationToken: cancellationToken);

            SetupIntentCreateOptions options = new()
            {
                Customer = stripeCustomerId,
            };

            SetupIntent setupIntent = setupIntentService.Create(options);

            SetupIntentResponse response = new()
            {
                ClientSecret = setupIntent.ClientSecret,
                ReturnURL = "cherami://",
                MerchantDisplayName = "Cher Ami",
                AllowsDelayedPaymentMethods = false,
            };

            await Send.OkAsync(response, cancellationToken);
        }
    }
}