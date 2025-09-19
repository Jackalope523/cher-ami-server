using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Boundaries
{
    #region Schemas

    public enum PaymentProvider
    {
        Stripe = 0,
        PayPal = 1,
        ApplePay = 2,
        GooglePay = 3,
    }

    public record CorePaymentMethod(long Id, PaymentProvider Provider,
        string ProviderCustomerId, string ProviderPaymentMethodId,
        string Last4Digits, string CardBrand, DateTime Expiry, Address BillingAddress);

    public record PaymentMethodShard(string Last4Digits, string CardBrand, DateTime Expiry, Address BillingAddress);


    public enum OrderStatus
    {
        Upcoming = 0,
        PendingPayment = 1,
        Processing = 2,
        Delivered = 3,
        Cancelled = 4,
    }

    public record CoreOrder(long Id, long IssueId, IssueType Type, long RecipientId, OrderStatus Status);

    public record OrderShard();

    #endregion

    #region Gates

    public interface IOrderRepository
    {
        Task<CorePaymentMethod> GetPaymentMethodForUserAsync(long userId);

        Task<CorePaymentMethod> AddPaymentMethodAsync(long userId);
        Task DeletePaymentMethodAsync(long paymentId);

        Task<List<CoreOrder>> GetOrdersForCircleAsync(long circleId);
        Task<List<CoreOrder>> GetOrdersForIssueAsync(long issueId);
    }

    public interface IOrderOperations
    {
        Task<PaymentMethodShard> GetPaymentMethodForUserAsync(long userId);

        Task<PaymentMethodShard> AddPaymentMethodAsync(long userId);
        Task RemovePaymentMethodAsync(long userId, long paymentId);

        Task<List<OrderShard>> GetOrdersForCircleAsync(long userId, long circleId);
        Task<List<OrderShard>> GetOrdersForIssueAsync(long userId, long issueId);
    }

    #endregion
}

