using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Core.Boundaries;
using Frontier.Controllers;

namespace Frontier.Stores
{
	public class UserAccountStore : IUserStore<CoreUser>,
		IUserPhoneNumberStore<CoreUser>,
		IUserEmailStore<CoreUser>,
		IUserSecurityStampStore<CoreUser>,
		IUserLockoutStore<CoreUser>
	{
		private IAccountOperations accounts { get; init; }

		public UserAccountStore(GuardBox box)
		{
			accounts = box.accounts;
		}

		public async Task<IdentityResult> CreateAsync(CoreUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			await accounts.CreateUserAsync(user.PhoneNumber, user.Email, user.Name, user.DateOfBirth);
			return IdentityResult.Success;
		}

		public async Task<IdentityResult> DeleteAsync(CoreUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			await accounts.DeleteUserAsync(user.Id);
			return IdentityResult.Success;
		}

		public async Task<CoreUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return await accounts.GetCoreUserAsync(GetId(userId));
		}

		public async Task<CoreUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return await accounts.GetCoreUserAsync(normalizedUserName);
		}

		public async Task<string> GetNormalizedUserNameAsync(CoreUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return await GetPhoneNumberAsync(user, cancellationToken);
		}

		public Task<string> GetUserIdAsync(CoreUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(user.Id.ToString());
		}

		public async Task<string> GetUserNameAsync(CoreUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return await GetPhoneNumberAsync(user, cancellationToken);
		}

		public Task SetNormalizedUserNameAsync(CoreUser user, string normalizedName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(0);
		}

		public Task SetUserNameAsync(CoreUser user, string userName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(0);
		}

		public async Task<IdentityResult> UpdateAsync(CoreUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			// No-Op'ing here because all operations that would update the user here
			// write to the database immediately instead of just locally.
			// This is based on our current database implementation but is subject to
			// change with a distributed architecture or different framework.
			return IdentityResult.Success;
		}

		public async Task SetPhoneNumberAsync(CoreUser user, string phoneNumber, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			await accounts.EditUserAsync(user.Id, phoneNumber: phoneNumber);
		}

		public Task<string> GetPhoneNumberAsync(CoreUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(user.PhoneNumber);
		}

		public Task<bool> GetPhoneNumberConfirmedAsync(CoreUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(user.IsPhoneConfirmed);
		}

		public async Task SetPhoneNumberConfirmedAsync(CoreUser user, bool confirmed, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			await accounts.EditUserAsync(user.Id, isPhoneNumberConfirmed: confirmed);
		}

		public async Task SetEmailAsync(CoreUser user, string email, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			await accounts.EditUserAsync(user.Id, email: email);
		}

		public Task<string> GetEmailAsync(CoreUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(user.Email);
		}

		public Task<bool> GetEmailConfirmedAsync(CoreUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(user.IsEmailConfirmed);
		}

		public async Task SetEmailConfirmedAsync(CoreUser user, bool confirmed, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			await accounts.EditUserAsync(user.Id, isEmailConfirmed: confirmed);
		}

		public async Task<CoreUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return await accounts.GetCoreUserAsync(normalizedEmail);
		}

		public Task<string> GetNormalizedEmailAsync(CoreUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(user.Email);
		}

		public Task SetNormalizedEmailAsync(CoreUser user, string normalizedEmail, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(0);
		}

		public async Task SetSecurityStampAsync(CoreUser user, string stamp, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			await accounts.EditUserAsync(user.Id, securityStamp: stamp);
		}

		public Task<string> GetSecurityStampAsync(CoreUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(user.SecurityStamp);
		}

		public Task<DateTimeOffset?> GetLockoutEndDateAsync(CoreUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(user.LockoutDate);
		}

		public async Task SetLockoutEndDateAsync(CoreUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (!lockoutEnd.HasValue)
			{
				throw new ArgumentException("Lockout null.");
			}
			await accounts.EditUserAsync(user.Id, lockoutDate: lockoutEnd.Value);
		}

		public async Task<int> IncrementAccessFailedCountAsync(CoreUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			int currentTries = user.AccessTries;
			await accounts.EditUserAsync(user.Id, accessTries: currentTries + 1);
			return currentTries + 1;
		}

		public async Task ResetAccessFailedCountAsync(CoreUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			await accounts.EditUserAsync(user.Id, accessTries: 0);
		}

		public Task<int> GetAccessFailedCountAsync(CoreUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(user.AccessTries);
		}

		public Task<bool> GetLockoutEnabledAsync(CoreUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(true);
		}

		public Task SetLockoutEnabledAsync(CoreUser user, bool enabled, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(0);
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		private long GetId(string id)
		{
			if (!long.TryParse(id, out long parsedId))
			{
				throw new ArgumentException("Not a valid long.", nameof(id));
			}

			return parsedId;
		}
	}
}
