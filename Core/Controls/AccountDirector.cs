using Core.Boundaries;
using Core.Entities;
using Core.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using static Core.Entities.Arbiter;
using static Core.Entities.Artificer;
using static Core.Entities.Psijic;

namespace Core.Controls
{
    internal class AccountDirector : AbstractDirector, IAccountOperations
    {
		#region Initialisation

		public AccountDirector(CoreTerminal terminal) : base(terminal) { }

		#endregion

		#region Operations

        public async Task<bool> GetUserExistsAsync(string phoneNumber)
        {
            return await Accounts.PhoneNumberExistsAsync(phoneNumber);
        }

		public async Task<CoreUser> GetCoreUserAsync(long userId)
        {
            return (await GetUserAsync(userId)).ToCoreUser();
        }

        public async Task<CoreUser> GetCoreUserAsync(string phoneNumber)
		{
            // Verify phone number is valid
            Verify(ContentValidation.TryNormalisePhoneNumber(phoneNumber, out string normalisedPhoneNumber),
                new UserErrorException(AccountErrorCode.INVALID_PHONE_NUMBER));

            return (await GetUser(normalisedPhoneNumber)).ToCoreUser();
		}

		public async Task<AccountShard> GetAccountShardAsync(long userId)
        {
            return (await GetUserAsync(userId)).ToAccountShard();
        }

		public async Task<UserShard> GetUserShardAsync(long userId)
        {
            return (await GetUserAsync(userId)).ToUserShard();
        }

        public async Task CreateUserAsync(string phoneNumber, string email, string title, string givenName, string familyName, DateTimeOffset dateOfBirth)
        {
            User newUser = new()
            {
                PhoneNumber = phoneNumber,
                Email = email,
                Title = title,
                GivenName = givenName,
                FamilyName = familyName,
                DateOfBirth = dateOfBirth,
                JoinDate = Time
            };

            // Validate and normalise user
            Verify(newUser.ValidateAndNormalise(out string issues),
                new UserErrorException(AccountErrorCode.INVALID_DETAILS, new { issues }));

            // Verify phone number is not in use
            await ThrowIfPhoneNumberTaken(newUser.PhoneNumber);

            // Check if email is in use
            if (!string.IsNullOrEmpty(email))
            { await ThrowIfEmailTaken(newUser.Email); }

            // Store created user
            var user = await Accounts.CreateUserAsync(newUser.PhoneNumber, email, newUser.Email,
                newUser.Title, newUser.GivenName, newUser.FamilyName, newUser.DateOfBirth, Time,
                Guid.NewGuid());
        }

        public async Task EditUserAsync(long userId,
            string phoneNumber = null, string email = null,
            string title = null, string givenName = null, string familyName = null,
			DateTimeOffset? dateOfBirth = null, bool? isPhoneNumberConfirmed = null, bool? isEmailConfirmed = null,
			string securityStamp = null, DateTimeOffset? lockoutDate = null, int? accessTries = null)
        {
            // Throws if user not found or locked
            var user = await base.GetUserAsync(userId);
            
            // Check unique details changed to avoid errors
            bool phoneNumberChanged = !string.IsNullOrEmpty(phoneNumber) && user.PhoneNumber != phoneNumber;
            bool emailChanged = !string.IsNullOrEmpty(email) && user.Email != email;
            bool dateOfBirthChanged = dateOfBirth.HasValue;
            bool titleChanged = !string.IsNullOrEmpty(title);
            bool givenNameChanged = !string.IsNullOrEmpty(givenName);
            bool familyNameChanged = !string.IsNullOrEmpty(familyName);

            // Modify user for validation
            user.PhoneNumber = phoneNumberChanged ? phoneNumber : user.PhoneNumber;
            user.Email = emailChanged ? email : user.Email;
            user.Title = titleChanged ? title : user.Title;
            user.GivenName = givenNameChanged ? givenName : user.GivenName;
            user.FamilyName = familyNameChanged ? familyName : user.FamilyName;
            user.DateOfBirth = dateOfBirthChanged ? dateOfBirth.Value : user.DateOfBirth;

            // Validate and Normalise
            Verify(user.ValidateAndNormalise(out string issues),
                new UserErrorException(AccountErrorCode.INVALID_DETAILS, new { issues }));

            List<(string Property, object Value)> edits = new();

            // Gather individual edits
			if (phoneNumberChanged)
            {
                await ThrowIfPhoneNumberTaken(user.PhoneNumber);
                edits.Add((nameof(CoreUser.PhoneNumber), user.PhoneNumber));
                // edits.Add((nameof(CoreUser.IsPhoneConfirmed), false));
            }
			if (emailChanged)
			{
                await ThrowIfEmailTaken(user.Email);
                edits.Add((nameof(CoreUser.Email), email));
                edits.Add(("NormalisedEmail", user.Email));
                edits.Add((nameof(CoreUser.IsEmailConfirmed), false));
            }
			if (titleChanged)
			{
                edits.Add((nameof(CoreUser.Title), user.Title));
			}
			if (givenNameChanged)
			{
                edits.Add((nameof(CoreUser.GivenName), user.GivenName));
			}
			if (familyNameChanged)
			{
                edits.Add((nameof(CoreUser.FamilyName), user.FamilyName));
			}
			if (dateOfBirthChanged)
			{
                edits.Add((nameof(CoreUser.DateOfBirth), user.DateOfBirth));
			}
            // Internal attributes for account store
			if (IsNotNull(isPhoneNumberConfirmed))
			{
                edits.Add((nameof(CoreUser.IsPhoneConfirmed), isPhoneNumberConfirmed.Value));
			}
			if (IsNotNull(isEmailConfirmed))
			{
                edits.Add((nameof(CoreUser.IsEmailConfirmed), isEmailConfirmed.Value));
			}
			if (!string.IsNullOrEmpty(securityStamp))
			{
                edits.Add((nameof(CoreUser.SecurityStamp), securityStamp));
			}
			if (IsNotNull(lockoutDate))
			{
                edits.Add((nameof(CoreUser.LockoutDate), lockoutDate.Value));
			}
			if (IsNotNull(accessTries))
			{
                edits.Add((nameof(CoreUser.AccessTries), accessTries.Value));
			}

            // Push update
            await Accounts.UpdateUserAsync(user.Id, edits);
		}

        public async Task UpdateUserAgreementAsync(long userId)
        {
            var user = await GetUserAsync(userId);

            await Accounts.UpdateUserAsync(user.Id,
                new() { (nameof(CoreUser.TimeOfUserAgreement), Time) });
        }

        public async Task EditAvatarAsync(long userId, MemoryStream image)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteUserAsync(long userId)
        {
            // TODO Gracefully delete data?
            await Accounts.DeleteUserAsync(userId);
        }

		#endregion

		#region Favours

		#endregion

		#region Tools

		private async Task<User> GetUser(string phoneNumber)
        {
            User user;

            try
            {
                user = new(await Accounts.GetUserByPhoneNumberAsync(phoneNumber));
            }
            catch
            { throw new UserErrorException(AccountErrorCode.NOT_FOUND); }

            // Check if user account is locked
            FailIf(user.IsLocked,
                new UserErrorException(AccountErrorCode.LOCKED));

            return user;
        }

		private async Task ThrowIfPhoneNumberTaken(string phoneNumber)
        {
			bool numberTaken = false;
			try
			{
				// Throws an exception if there is no user
				await GetUser(phoneNumber);
				numberTaken = true;
			}
			catch { }

            FailIf(numberTaken,
                new UserErrorException(AccountErrorCode.PHONE_NUMBER_EXISTS));
		}

        private async Task ThrowIfEmailTaken(string normalisedEmail)
        {
			bool emailTaken = false;
			try
			{
                // Throws an exception if there is no user
                await Accounts.GetUserByEmailAsync(normalisedEmail);
				emailTaken = true;
			}
			catch { }

			FailIf(emailTaken,
                new UserErrorException(AccountErrorCode.EMAIL_EXISTS));
        }

		#endregion
	}
}
