using CherAmiAPI.Services;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.PaymentMethods
{
    public class UpdatePaymentMethodEndpoint(BillingService billingService) : EndpointWithoutRequest<SetupIntentResponse>
    {
        public override void Configure()
        {
            Patch("/payment-method");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            SetupIntentResponse response = await billingService.ReplacePaymentMethodAsync(userId, cancellationToken);

            await Send.OkAsync(response, cancellationToken);
        }
    }
}
