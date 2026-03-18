using CherAmiAPI.Contexts;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Recipients
{
    public class GetPriceEndpoint(IConfiguration config, ApplicationDbContext ctx, PriceService priceService) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/recipient/price");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool isBillingExempt = await ctx.Users.Where(x => x.Id == userId).Select(x => x.IsBillingExempt).SingleAsync(cancellationToken: cancellationToken);

            if (isBillingExempt)
            {
                await Send.OkAsync(0L, cancellationToken);
            }
            else
            {
                Price price = await priceService.GetAsync(config["MONTHLY_MAGAZINE_STANDARD_PRICE_ID"], cancellationToken: cancellationToken);
                await Send.OkAsync(price.UnitAmount, cancellationToken);
            }
        }
    }
}