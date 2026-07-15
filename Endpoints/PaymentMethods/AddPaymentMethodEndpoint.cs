using CherAmiAPI.Services;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.PaymentMethods
{
    public class AddPaymentMethodEndpoint(BillingService billingService) : EndpointWithoutRequest<SetupIntentResponse>
    {
        public override void Configure()
        {
            Post("/payment-method");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            SetupIntentResponse response = await billingService.CreateSetupIntentAsync(userId, cancellationToken);

            await Send.OkAsync(response, cancellationToken);
        }
    }
}
