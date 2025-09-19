using CrazyLizard.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Boundaries
{
	#region Schemas

	public enum UserAccountStatus
	{ Active, Limited, Suspended, Blacklisted }
	
    #endregion

    #region Gates

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
        Task AddRecipientAsync(CoreRecipient recipient);
        Task UpdateRecipientAsync(long recipientId, List<(string Property, object Value)> edits);
        Task RemoveRecipientAsync(long recipientId);
    }

	public interface IAccountService
	{
		Task<bool> UserExistsAsync(string phoneNumber);

		Task<User> GetCoreUserAsync(long userId);
		Task<User> GetCoreUserAsync(string phoneNumber);

		Task CreateUserAsync(string phoneNumber, string email,
			string title, string givenName, string familyName,
			DateOnly dateOfBirth);
		Task EditUserAsync(long userId,
			string phoneNumber = null, string email = null,
			string title = null, string givenName = null, string familyName = null,
			DateOnly? dateOfBirth = null, bool? isPhoneNumberConfirmed = null, bool? isEmailConfirmed = null,
			string securityStamp = null, DateTimeOffset? lockoutDate = null, int? accessTries = null);
		Task EditAvatarAsync(long userId, MemoryStream image);
		Task UpdateUserAgreementAsync(long userId);
		Task DeleteUserAsync(long userId);

        Task AddRecipientAsync(long userId, CoreRecipient recipient, CancellationToken cancellationToken = default);
        Task RemoveRecipientAsync(long userId, long recipientId, CancellationToken cancellationToken = default);
        Task EditRecipientAsync(long userId, long recipientId, List<(string Property, object Value)> edits);

        Task<string> CreateSetupIntentAsync(long userId, CancellationToken cancellationToken = default);
		Task UpdateStripeCustomerIdAsync(long userId, string newId);
        Task UpdateStripeSubscriptionIdAsync(long userId, string newId);
        Task ConfirmPaymentDetailsProvidedAsync(string stripeClientId);
    }

	public interface IEmailService
	{
		Task SendEmailAsync(string email, string subject, string body);
	}

	public interface ISMSService
	{
		Task SendTextMessageAsync(string phoneNumber, string message);
        Task SendWhatsAppAuthMessageAsync(string phoneNumber, string code);
    }

	#endregion
}
