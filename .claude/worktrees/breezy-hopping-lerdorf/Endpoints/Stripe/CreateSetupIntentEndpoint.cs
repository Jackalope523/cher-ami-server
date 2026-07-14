using CherAmiAPI.Contexts;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Stripe
{
    public record CreateSetupIntentResponse
    {
        public string ClientSecret { get; set; }
        public string ReturnURL { get; set; }
        public string MerchantDisplayName { get; set; }
        public bool AllowsDelayedPaymentMethods { get; set; }
    }
    
    public class CreateSetupIntentEndpoint(ApplicationDbContext ctx, SetupIntentService setupIntentService) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Post("/stripe/setup-intents");
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

            CreateSetupIntentResponse response = new()
            {
                ClientSecret = setupIntent.ClientSecret,
                ReturnURL = "cherami://",
                MerchantDisplayName = "Cher Ami",
                AllowsDelayedPaymentMethods = true,
            };

             await Send.OkAsync(response, cancellationToken);
        }
    }
}