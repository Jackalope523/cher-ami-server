using CherAmiAPI.Contexts;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Stripe;
using System.Linq;
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

    public class GetPriceEndpointV2(IConfiguration config, ApplicationDbContext ctx, PriceService priceService) : EndpointWithoutRequest<PriceResponse>
    {
        public override void Configure()
        {
            Get("/v2/recipient/price");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool isBillingExempt = await ctx.Users.Where(x => x.Id == userId).Select(x => x.IsBillingExempt).SingleAsync(cancellationToken: cancellationToken);

            if (isBillingExempt)
            {
                PriceResponse response = new()
                {
                    StandardEditionPrice = 0L,
                    MilitaryEditionPrice = 0L
                };

                await Send.OkAsync(response, cancellationToken);
            }
            else
            {
                Price standardPrice = await priceService.GetAsync(config["MONTHLY_MAGAZINE_STANDARD_PRICE_ID"], cancellationToken: cancellationToken);
                Price militaryPrice = await priceService.GetAsync(config["MONTHLY_MAGAZINE_MILITARY_PRICE_ID"], cancellationToken: cancellationToken);

                PriceResponse response = new()
                {
                    StandardEditionPrice = (long)standardPrice.UnitAmount,
                    MilitaryEditionPrice = (long)militaryPrice.UnitAmount
                };

                await Send.OkAsync(response, cancellationToken);
            }
        }
    }
}