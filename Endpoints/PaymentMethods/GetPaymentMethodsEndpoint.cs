using CherAmiAPI.Contexts;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.PaymentMethods
{
    public record CardDTO
    {
        public string Id { get; set; }
        public string DisplayBrand { get; set; }
        public string Last4 { get; set; }
    }

    public class CardDTOMapper : ResponseMapper<CardDTO, PaymentMethod>
    {
        public override CardDTO FromEntity(PaymentMethod paymentMethod)
        {
            return new CardDTO
            {
                Id = paymentMethod.Id,
                DisplayBrand = paymentMethod.Card.Brand,
                Last4 = paymentMethod.Card.Last4,
            };
        }
    }


    public class GetPaymentMethodsEndpoint(ApplicationDbContext ctx, CustomerPaymentMethodService customerPaymentMethodService) : EndpointWithoutRequest<List<CardDTO>, CardDTOMapper>
    {
        public override void Configure()
        {
            Get("/payment-methods");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            string stripeCustomerId = await ctx.Users
                                      .Where(x => x.Id == userId)
                                      .Select(x => x.StripeCustomerId)
                                      .SingleAsync(cancellationToken: cancellationToken);

            List<PaymentMethod> paymentMethods = (await customerPaymentMethodService.ListAsync(stripeCustomerId, cancellationToken: cancellationToken)).Data;

            await Send.OkAsync([.. paymentMethods.Select(Map.FromEntity)], cancellationToken);
        }
    }
}