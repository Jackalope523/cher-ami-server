using CherAmiAPI.Services;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.PaymentMethods
{
    public class RemovePaymentMethodEndpoint(BillingService billingService) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Delete("/payment-method");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await billingService.RemovePaymentMethodAsync(userId, cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
