using CrazyLizard.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrazyLizard.Boundaries.Repository
{
    public interface IAccountRepository
	{
		Task<bool> ShareCircle(long userId1, long userId2);
		Task<bool> PhoneNumberExistsAsync(string phoneNumber);
		Task<bool> EmailExistsAsync(string normalisedEmail);

		Task<User> GetUserByIdAsync(long userId);
        Task<User> GetUserByPhoneNumberAsync(string phoneNumber);
		Task<User> GetUserByEmailAsync(string normalisedEmail);

		Task<User> CreateUserAsync(string phoneNumber, string email, string normalisedEmail,
			string title, string givenName, string familyName,
			DateOnly dateOfBirth, DateTimeOffset joinDate, Guid notificationId);
		Task UpdateUserAsync(long userId, List<(string Property, object Value)> edits);
		Task DeleteUserAsync(long userId);

		Task UpdateStripeCustomerIdAsync(long userId, string newId);
        Task UpdateStripeSubscriptionIdAsync(long userId, string newId);
        Task ConfirmPaymentDetailsProvidedAsync(string stripeCustomerId);

        Task<bool> IsManagerAsync(long userId, long recipientId);
        Task AddRecipientAsync(Recipient recipient);
        Task UpdateRecipientAsync(long recipientId, List<(string Property, object Value)> edits);
        Task RemoveRecipientAsync(long recipientId);
    }
}
