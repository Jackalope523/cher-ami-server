using CherAmiAPI.Services;
using FastEndpoints;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using PaymentMethod = Stripe.PaymentMethod;

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


    public class GetPaymentMethodsEndpoint(BillingService billingService) : EndpointWithoutRequest<List<CardDTO>, CardDTOMapper>
    {
        public override void Configure()
        {
            Get("/payment-methods");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<PaymentMethod> paymentMethods = await billingService.GetPaymentMethodsAsync(userId, cancellationToken);

            await Send.OkAsync([.. paymentMethods.Select(Map.FromEntity)], cancellationToken);
        }
    }
}
