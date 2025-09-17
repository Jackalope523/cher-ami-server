using FastEndpoints;
using Stripe;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Stripe
{
    public class WebhookEndpoint : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Post("/stripe/webhook");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            string json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(cancellationToken);

            Event stripeEvent = EventUtility.ConstructEvent(
                json,
                HttpContext.Request.Headers["Stripe-Signature"],
                ""
            );

            if (stripeEvent.Type == "invoice.paid")
            {
         
            }

            if (stripeEvent.Type == "invoice.payment_failed")
            {
        
            }

            if (stripeEvent.Type == "customer.subscription.deleted")
            {
          
            }

            await Send.NoContentAsync(cancellationToken);
        }
    }
}