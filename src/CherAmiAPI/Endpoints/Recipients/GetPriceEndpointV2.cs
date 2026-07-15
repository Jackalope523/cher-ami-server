using CherAmiAPI.Services;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Recipients
{
    public class PriceResponse
    {
        public long StandardEditionPrice { get; set; }
        public long MilitaryEditionPrice { get; set; }
    }

    public class GetPriceEndpointV2(BillingService billingService) : EndpointWithoutRequest<PriceResponse>
    {
        public override void Configure()
        {
            Get("/v2/recipient/price");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            (long standardPrice, long militaryPrice) = await billingService.GetPricesAsync(userId, cancellationToken);

            PriceResponse response = new()
            {
                StandardEditionPrice = standardPrice,
                MilitaryEditionPrice = militaryPrice
            };

            await Send.OkAsync(response, cancellationToken);
        }
    }
}
