using FastEndpoints;
using System.Threading;
using System.Threading.Tasks;
using Stripe;

namespace CherAmiAPI.Endpoints.Recipients
{
    public class GetPriceEndpoint(PriceService priceService) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/recipient/price");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            Price price = await priceService.GetAsync("price_1S7govARYKi6NXMeuiOwG70F", cancellationToken: cancellationToken);
            await Send.OkAsync(price.UnitAmount, cancellationToken);
        }
    }
}