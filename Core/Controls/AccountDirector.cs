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
            return await Accounts.UserExistsAsync(phoneNumber);
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

        public async Task CreateUserAsync(string phoneNumber, string email, string name, DateTimeOffset dateOfBirth)
        {
            User newUser = new()
            {
                PhoneNumber = phoneNumber,
                Email = email,
                Name = name,
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

            // Store nest
            var user = await Accounts.CreateUserAsync(newUser.PhoneNumber, email, newUser.Email,
                newUser.Name, newUser.DateOfBirth, Time,
                CharacterVector.Default(newUser.GetAge()).ToCharacter(),
                Guid.NewGuid());
        }

        public async Task EditUserAsync(long userId,
            string phoneNumber = null, string email = null, string name = null,
			bool? isPhoneNumberConfirmed = null, bool? isEmailConfirmed = null,
			string securityStamp = null, DateTimeOffset? lockoutDate = null, int? accessTries = null)
        {
            // Throws if user not found or locked
            var user = await base.GetUserAsync(userId);
            
            // Check unique details changed to avoid errors
            bool phoneNumberChanged = !string.IsNullOrEmpty(phoneNumber) && user.PhoneNumber != phoneNumber;
            bool emailChanged = !string.IsNullOrEmpty(email) && user.Email != email;
            bool nameChanged = !string.IsNullOrEmpty(name);

            // Modify user for validation
            user.PhoneNumber = phoneNumberChanged ? phoneNumber : user.PhoneNumber;
            user.Email = emailChanged ? email : user.Email;
            user.Name = nameChanged ? name : user.Name;

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
			if (nameChanged)
			{
                edits.Add((nameof(CoreUser.Name), user.Name));
			}
			if (emailChanged)
			{
                await ThrowIfEmailTaken(user.Email);
                edits.Add((nameof(CoreUser.Email), email));
                edits.Add(("NormalisedEmail", user.Email));
                edits.Add((nameof(CoreUser.IsEmailConfirmed), false));
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
            var user = await GetUserAsync(userId);

            await Terminal.MediaDirector.UploadAvatarAsync(user.Id, image);
        }

        public async Task<string> RerollCodeAsync(long userId)
        {
            var user = await GetUserAsync(userId);

            return await Accounts.RerollUserCodeAsync(user.Id);
        }

        public async Task DeleteUserAsync(long userId)
        {
            // TODO Gracefully delete data
            await Accounts.SoftDeleteAsync(userId);
        }

        public async Task UpdateUserLocationAsync(long userId, double latitude, double longitude)
		{
			var user = await base.GetUserAsync(userId);
            var userIsAtGathering = user.IsAtGathering();

            user.LastKnownLocation.Set(new() { Latitude = latitude, Longitude = longitude });
            await user.HandleHaunt();

            Log.LogWarning("Updating location for user {id} {name} to {latitude}, {longitude} at {time}",
                user.Id, user.Name, latitude, longitude, Time);

            // Position update
            _ = Accounts.UpdateRecentLocationAsync(user.Id,
                (await user.LastKnownLocation).Latitude,
                (await user.LastKnownLocation).Longitude,
                (await user.LastKnownRadius).Metres);
            // Haunt update
            _ = Accounts.UpdateHauntAsync(user.Id,
                (await user.Haunt).Latitude,
                (await user.Haunt).Longitude,
                (await user.HauntRadius).Metres,
                await user.HauntStability);

            var nextGathering = await user.NextGathering();

            // Check if user is at an gathering
            if (await userIsAtGathering)
            {
                var ongoingGatherings = await user.OngoingGatherings;

                foreach (var current in ongoingGatherings)
                {
                    // Check if user is in the gathering radius
                    if (GeoLocation.AreInRange(await user.LastKnownLocation, current.Location, current.Radius))
                    {
                        await Gatherings.UpdateGatheringAsync(nextGathering.Id, new() { (nameof(CoreGathering.Decay), Gathering.InitialDecay) });
                    }
                }
            }
            // Check if user is on their way to an gathering
            else if (!await userIsAtGathering &&
                !nextGathering.Equals(Gathering.None))
            {
                // Check if user is close enough to be arrived
                if (nextGathering.IsOngoing &&
                    await nextGathering.IsInRange(user))
                {
                    Log.LogWarning("Guest {name} entered gathering {title} area, marking as arrived...", user.Name, nextGathering.Title);
                    await Gatherings.SetUserStateAsync(user.Id, nextGathering.Id, GatheringBond.Arrived, Time);
                    await Gatherings.UpdateGatheringAsync(nextGathering.Id, new() { (nameof(CoreGathering.Decay), Gathering.InitialDecay) });
                }
            }
        }

		#endregion

		#region Favours

        internal async Task UpdateAllAsync(List<User> users, Func<User,List<(string Property, object Value)>> edits)
        {
            users.ForEach(user => Accounts.UpdateUserAsync(user.Id, edits(user)));
		}

        internal async Task<(GeoLocation Location, Distance Radius, int Stability)>
            RequestUserHauntAsync(User user)
        {
            var result = await Accounts.GetUserHauntAsync(user.Id);
            return (new() { Latitude = result.Latitude, Longitude = result.Longitude }, new() { Metres = result.Radius }, result.Stability);
        }

        internal async Task<(GeoLocation Location, Distance Radius)>
            RequestLastKnownUserLocationAsync(User user)
        {
            var result = await Accounts.GetRecentLocationAsync(user.Id);

            if (result == null)
            { return (GeoLocation.None, Distance.None); }

            return (new() { Latitude = result.Latitude, Longitude = result.Longitude }, new() { Metres = result.Radius });
        }

		#endregion

		#region Tools

		private async Task<User> GetUser(string phoneNumber)
        {
            User user;

            try
            {
                user = new(await Accounts.FindUserByPhoneNumberAsync(phoneNumber));
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
                await Accounts.FindUserByEmailAsync(normalisedEmail);
				emailTaken = true;
			}
			catch { }

			FailIf(emailTaken,
                new UserErrorException(AccountErrorCode.EMAIL_EXISTS));
        }

		#endregion
	}
}
