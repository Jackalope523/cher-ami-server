using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Core.Boundaries;

using static Core.Entities.Arbiter;

namespace Core.Controls
{
    internal class OrderDirector : AbstractDirector, IOrderOperations
	{
		#region Initialisation

		public OrderDirector(CoreTerminal terminal) : base(terminal) { }

        #endregion

        #region Operations

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

        #endregion

        #region Favours


        #endregion
    }
}
