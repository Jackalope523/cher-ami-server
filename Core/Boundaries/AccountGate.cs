using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Core.Boundaries
{
	#region Schemas

	public enum UserAccountStatus
	{ Active, Limited, Suspended, Blacklisted }

	public record CoreUser(long Id, string PhoneNumber, string Email, string NormalisedEmail,
		string Title, string FirstName, string LastName, DateTimeOffset DateOfBirth,
		bool IsPhoneConfirmed, bool IsEmailConfirmed, bool IsPendingDeletion,
		string SecurityStamp, DateTimeOffset? LockoutDate, int AccessTries, UserAccountStatus AccountStatus,
		DateTimeOffset JoinDate, DateTimeOffset TimeOfUserAgreement, Guid NotificationId)
		: CoreOnlyData();

	public record AccountShard(long Id, string PhoneNumber, string Email,
		string Title, string FirstName, string LastName, DateTimeOffset DateOfBirth,
		bool IsPhoneConfirmed, bool IsEmailConfirmed, UserAccountStatus AccountStatus,
		DateTimeOffset JoinDate, DateTimeOffset TimeOfUserAgreement, Guid NotificationId);

    public record UserShard(long Id, string Name);
	
    #endregion

    #region Gates

    public interface IAccountDatabase
	{
		Task<bool> PhoneNumberExistsAsync(string phoneNumber);
		Task<bool> EmailExistsAsync(string normalisedEmail);

		Task<CoreUser> GetUserByIdAsync(long userId);
        Task<CoreUser> GetUserByPhoneNumberAsync(string phoneNumber);
		Task<CoreUser> GetUserByEmailAsync(string normalisedEmail);

		Task<CoreUser> CreateUserAsync(string phoneNumber, string email, string normalisedEmail,
			string title, string firstName, string lastName,
			DateTimeOffset dateOfBirth, DateTimeOffset joinDate, Guid notificationId);
		Task UpdateUserAsync(long userId, List<(string Property, object Value)> edits);

		Task SoftDeleteAsync(long userId);
        Task HardDeleteAsync(long userId);
    }

	public interface IAccountOperations
	{
		Task<bool> GetUserExistsAsync(string phoneNumber);

		Task<CoreUser> GetCoreUserAsync(long userId);
		Task<CoreUser> GetCoreUserAsync(string phoneNumber);
		Task<AccountShard> GetAccountShardAsync(long userId);
		Task<UserShard> GetUserShardAsync(long userId);

		Task CreateUserAsync(string phoneNumber, string email,
			string title, string firstName, string lastName,
			DateTimeOffset dateOfBirth);
		Task EditUserAsync(long userId,
			string phoneNumber = null, string email = null,
			string title = null, string firstName = null, string lastName = null,
			DateTimeOffset? dateOfBirth = null, bool? isPhoneNumberConfirmed = null, bool? isEmailConfirmed = null,
			string securityStamp = null, DateTimeOffset? lockoutDate = null, int? accessTries = null);
		Task EditAvatarAsync(long userId, MemoryStream image);
		Task UpdateUserAgreementAsync(long userId);
		Task DeleteUserAsync(long userId);
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
