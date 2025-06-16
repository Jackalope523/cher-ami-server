using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Core.Boundaries
{
	#region Schemas

	public enum UserAccountStatus
	{ Active, Impotent, Limited, Suspended, Blacklisted }

	public record CoreUser(long Id, string PhoneNumber, string Email, string Name, string Code,
		DateTimeOffset DateOfBirth, bool IsPhoneConfirmed, bool IsEmailConfirmed, bool IsPendingDeletion,
		string SecurityStamp, DateTimeOffset? LockoutDate, int AccessTries, UserAccountStatus AccountStatus,
		DateTimeOffset JoinDate, int Reputation, CharacterShard Character, DateTimeOffset TimeOfUserAgreement,
		Guid NotificationId)
		: CoreOnlyData();

	public record AccountShard(long Id, string PhoneNumber, string Email, string Name, string Code,
        DateTimeOffset DateOfBirth, bool IsPhoneConfirmed, bool IsEmailConfirmed,
		UserAccountStatus AccountStatus, DateTimeOffset JoinDate, DateTimeOffset TimeOfUserAgreement,
		Guid NotificationId);

    public record UserShard(long Id, string Name);

    public record CharacterShard(int Age, int Extraversion, int Athleticism, int Chaoticness,
		int Competitiveness, int Industriousness, int NightOwl, int Openness);

    public record LocationShard(double Latitude, double Longitude, double Radius);
    public record HauntShard(double Latitude, double Longitude, double Radius, int Stability);
	
    #endregion

    #region Gates

    public interface IAccountDatabase
	{
		Task<bool> UserExistsAsync(string phoneNumber);

		Task<CoreUser> FindUserByIdAsync(long userId);
        Task<CoreUser> FindUserByPhoneNumberAsync(string phoneNumber);
		Task<CoreUser> FindUserByEmailAsync(string normalisedEmail);
		Task<CoreUser> CreateUserAsync(string phoneNumber, string email, string normalisedEmail,
			string name, DateTimeOffset dateOfBirth, DateTimeOffset joinDate, CharacterShard character, Guid notificationId);
		Task UpdateUserAsync(long userId, List<(string Property, object Value)> edits);

		Task<LocationShard> GetRecentLocationAsync(long userId);
		Task UpdateRecentLocationAsync(long userId, double latitude, double longitude, double radius);

		Task<HauntShard> GetUserHauntAsync(long userId);
		Task UpdateHauntAsync(long userId, double latitude, double longitude, double radius, int stability);

		Task<string> RerollUserCodeAsync(long userId);
		Task<CoreUser> FindUserByCodeAsync(string code);

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

		Task CreateUserAsync(string phoneNumber, string email, string name,
			DateTimeOffset dateOfBirth);
		Task EditUserAsync(long userId,
			string phoneNumber = null, string email = null, string name = null,
			bool? isPhoneNumberConfirmed = null, bool? isEmailConfirmed = null,
			string securityStamp = null, DateTimeOffset? lockoutDate = null, int? accessTries = null);
		Task UpdateUserAgreementAsync(long userId);
		Task<string> RerollCodeAsync(long userId);
		Task EditAvatarAsync(long userId, MemoryStream image);
		Task DeleteUserAsync(long userId);

		Task UpdateUserLocationAsync(long userId, double latitude, double longitude);
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
