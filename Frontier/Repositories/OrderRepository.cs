using Repository.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    class OrderRepository(LLContext ctx) : IOrderRepository
    {
        public Task<CorePaymentMethod> AddPaymentMethodAsync(long userId)
        {
            throw new NotImplementedException();
        }

        public Task DeletePaymentMethodAsync(long paymentId)
        {
            throw new NotImplementedException();
        }

        public Task<List<CoreOrder>> GetOrdersForCircleAsync(long circleId)
        {
            throw new NotImplementedException();
        }

        public Task<List<CoreOrder>> GetOrdersForIssueAsync(long issueId)
        {
            throw new NotImplementedException();
        }

        public Task<CorePaymentMethod> GetPaymentMethodForUserAsync(long userId)
        {
            throw new NotImplementedException();
        }
    }
}
