using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Boundaries;

namespace CrazyLizard.Services
{
    internal class OrderService(IOrderRepository orderRepository) : IOrderOperations
	{
        public Task<PaymentMethodShard> AddPaymentMethodAsync(long userId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<OrderShard>> GetOrdersForCircleAsync(long userId, long circleId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<OrderShard>> GetOrdersForIssueAsync(long userId, long issueId)
        {
            throw new System.NotImplementedException();
        }

        public Task<PaymentMethodShard> GetPaymentMethodForUserAsync(long userId)
        {
            throw new System.NotImplementedException();
        }

        public Task RemovePaymentMethodAsync(long userId, long paymentId)
        {
            throw new System.NotImplementedException();
        }
    }
}
