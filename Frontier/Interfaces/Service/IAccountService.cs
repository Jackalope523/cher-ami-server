using Core.Boundaries;
using CrazyLizard.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Boundaries.Service
{
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
}
