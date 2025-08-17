using Core.Boundaries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LazyLizardBackend.Services
{
    public class AccountService(IAccountRepository accountRepository) : IAccountService
    {
        public async Task<bool> GetUserExistsAsync(string phoneNumber)
        {
            return await accountRepository.PhoneNumberExistsAsync(phoneNumber);
        }

		public async Task<CoreUser> GetCoreUserAsync(long userId)
        {
            return await accountRepository.GetUserByIdAsync(userId);
        }

        public async Task<CoreUser> GetCoreUserAsync(string phoneNumber)
		{
            return await accountRepository.GetUserByPhoneNumberAsync(phoneNumber);
        }

        public async Task CreateUserAsync(string phoneNumber, string email, string title, string givenName, string familyName, DateTimeOffset dateOfBirth)
        {
            await accountRepository.CreateUserAsync(phoneNumber, email, email,
                title, givenName, familyName, dateOfBirth, DateTimeOffset.UtcNow,
                Guid.NewGuid());
        }

        public async Task EditUserAsync(long userId,
            string phoneNumber = null, string email = null,
            string title = null, string givenName = null, string familyName = null,
			DateTimeOffset? dateOfBirth = null, bool? isPhoneNumberConfirmed = null, bool? isEmailConfirmed = null,
			string securityStamp = null, DateTimeOffset? lockoutDate = null, int? accessTries = null)
        {
            // Throws if user not found or locked
            CoreUser user = await accountRepository.GetUserByIdAsync(userId);
            
            // Check unique details changed to avoid errors
            bool phoneNumberChanged = !string.IsNullOrEmpty(phoneNumber) && user.PhoneNumber != phoneNumber;
            bool emailChanged = !string.IsNullOrEmpty(email) && user.Email != email;
            bool dateOfBirthChanged = dateOfBirth.HasValue;
            bool titleChanged = !string.IsNullOrEmpty(title);
            bool givenNameChanged = !string.IsNullOrEmpty(givenName);
            bool familyNameChanged = !string.IsNullOrEmpty(familyName);

            // Modify user for validation
            string newPhoneNumber = phoneNumberChanged ? phoneNumber : user.PhoneNumber;
            string newEmail = emailChanged ? email : user.Email;
            string newTitle = titleChanged ? title : user.Title;
            string newGivenName = givenNameChanged ? givenName : user.FirstName;
            string newFamilyName = familyNameChanged ? familyName : user.LastName;
            DateTimeOffset newDateOfBirth = dateOfBirthChanged ? dateOfBirth.Value : user.DateOfBirth;

            List<(string Property, object Value)> edits = new();
            // Gather individual edits
			if (phoneNumberChanged)
            {
                edits.Add((nameof(CoreUser.PhoneNumber), user.PhoneNumber));
            }
			if (emailChanged)
			{

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
                edits.Add((nameof(CoreUser.FirstName), user.FirstName));
			}
			if (familyNameChanged)
			{
                edits.Add((nameof(CoreUser.LastName), user.LastName));
			}
			if (dateOfBirthChanged)
			{
                edits.Add((nameof(CoreUser.DateOfBirth), user.DateOfBirth));
			}
            // Internal attributes for account store
			if (isPhoneNumberConfirmed != null)
			{
                edits.Add((nameof(CoreUser.IsPhoneConfirmed), isPhoneNumberConfirmed.Value));
			}
			if (isEmailConfirmed != null)
			{
                edits.Add((nameof(CoreUser.IsEmailConfirmed), isEmailConfirmed.Value));
			}
			if (!string.IsNullOrEmpty(securityStamp))
			{
                edits.Add((nameof(CoreUser.SecurityStamp), securityStamp));
			}
			if (lockoutDate != null)
			{
                edits.Add((nameof(CoreUser.LockoutDate), lockoutDate.Value));
			}
			if (accessTries != null)
			{
                edits.Add((nameof(CoreUser.AccessTries), accessTries.Value));
			}

            // Push update
            await accountRepository.UpdateUserAsync(user.Id, edits);
		}

        public async Task UpdateUserAgreementAsync(long userId)
        {
            var user = await accountRepository.GetUserByIdAsync(userId);

            await accountRepository.UpdateUserAsync(user.Id,
                new() { (nameof(CoreUser.TimeOfUserAgreement), DateTimeOffset.UtcNow) });
        }

        public async Task EditAvatarAsync(long userId, MemoryStream image)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteUserAsync(long userId)
        {
            await accountRepository.DeleteUserAsync(userId);
        }
	}
}
