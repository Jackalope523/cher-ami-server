using CherAmiAPI.Services;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Recipients
{
    public class GetPriceEndpoint(BillingService billingService) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/recipient/price");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            long? price = await billingService.GetStandardPriceAsync(userId, cancellationToken);

            await Send.OkAsync(price, cancellationToken);
        }
    }
}
