using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Boundaries
{
    #region Schemas

    public enum OrderStatus
    { Pending, Paid, Printing, Delivered }

    public record CoreOrder(long Id, OrderStatus Status)
        : CoreOnlyData();

    public record OrderShard();


    public record CoreRecipient(long Id, string Name, string Address)
        : CoreOnlyData();

    public record RecipientShard();


    public record CorePaymentMethod(long Id)
        : CoreOnlyData();

    public record PaymentMethodShard();

    #endregion

    #region Gates

    public interface IOrderDatabase
    {
        Task<CorePaymentMethod> GetPaymentMethodForUserAsync(long userId);

        Task<CorePaymentMethod> AddPaymentMethodAsync(long userId);
        Task UpdatePaymentMethodAsync(long paymentId, List<(string Property, object Value)> edits);
        Task DeletePaymentMethodAsync(long paymentId);

        Task<List<CoreOrder>> GetOrdersForGroupAsync(long groupId);
        Task<List<CoreOrder>> GetOrdersForSegmentAsync(long segmentId);

        Task<List<CoreRecipient>> GetRecipientsForGroupAsync(long groupId);

        Task AddRecipientAsync(long groupId, long userId);
        Task UpdateRecipientAsync(long recipientId, List<(string Property, object Value)> edits);
        Task DeleteRecipientAsync(long recipientId);
    }

    public interface IOrderOperations
    {
    }

    #endregion
}

