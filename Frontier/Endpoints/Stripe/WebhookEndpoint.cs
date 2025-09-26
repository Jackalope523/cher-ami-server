using CrazyLizard.Interfaces.Service;
using FastEndpoints;
using Stripe;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Stripe
{
    public class WebhookEndpoint(IAccountService accountService) : EndpointWithoutRequest
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
                "whsec_02fbb145f2d79eec40f32c046015015b72529d6b386997767d7b038c0316e849"
            );

            if (stripeEvent.Type == "setup_intent.succeeded")
            {
                SetupIntent setupIntent = stripeEvent.Data.Object as SetupIntent;
                await accountService.ConfirmPaymentDetailsProvidedAsync(setupIntent.CustomerId);
            }

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