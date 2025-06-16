using System;
using System.Drawing;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Core.Boundaries;
using Core.Entities;
using Xunit;
using Xunit.Abstractions;

using NetTopologySuite.Utilities;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using Core.Notifications;

namespace Core.Tests
{
    public class UserHook : IAccountDatabase
    {
        private IAccountDatabase accounts;
        private ConcurrentBag<long> generatedUserIds;

        public UserHook(IAccountDatabase accountDatabase, ConcurrentBag<long> userIdList)
        {
            accounts = accountDatabase;
            generatedUserIds = userIdList;
        }

        public async Task<CoreUser> CreateUserAsync(string phoneNumber, string email, string normalisedEmail, string name, DateTimeOffset dateOfBirth, DateTimeOffset joinDate, CharacterShard character, Guid notificationId)
        {
			ContentValidation.TryNormalisePhoneNumber(phoneNumber, out phoneNumber);
            // Ensure no duplicate user exists
            CoreUser userCheck = null;
			try
			{
				userCheck = await accounts.FindUserByPhoneNumberAsync(phoneNumber);
				Console.Error.WriteLine($"Tried to create user with phone number {phoneNumber} but it already exists.");
			}
			catch { }

			if (userCheck != null)
			{
				// Check that user is properly inside id bag
				if (!generatedUserIds.Contains(userCheck.Id))
				{
					Console.Error.WriteLine("User did not exist in id bag. Adding...");
					generatedUserIds.Add(userCheck.Id);
				}

				throw new UnexpectedFailureException("User exists.");
			}
            
			await accounts.CreateUserAsync(phoneNumber, email, normalisedEmail, name, dateOfBirth, joinDate, character, notificationId);

			CoreUser createdUser = await accounts.FindUserByPhoneNumberAsync(phoneNumber);
			generatedUserIds.Add(createdUser.Id);

			return createdUser;
        }

        public async Task<bool> UserExistsAsync(string phoneNumber)
        {
            return await accounts.UserExistsAsync(phoneNumber);
        }

        public async Task<CoreUser> FindUserByCodeAsync(string code)
        {
            return await accounts.FindUserByCodeAsync(code);
        }

        public async Task<string> RerollUserCodeAsync(long userId)
        {
            return await accounts.RerollUserCodeAsync(userId);
        }

        public async Task<CoreUser> FindUserByEmailAsync(string normalisedEmail)
        {
			return await accounts.FindUserByEmailAsync(normalisedEmail);
        }

        public async Task<CoreUser> FindUserByIdAsync(long userId)
        {
			return await accounts.FindUserByIdAsync(userId);
        }

        public async Task<CoreUser> FindUserByPhoneNumberAsync(string phoneNumber)
        {
            ContentValidation.TryNormalisePhoneNumber(phoneNumber, out phoneNumber);
            return await accounts.FindUserByPhoneNumberAsync(phoneNumber);
        }

        public async Task<LocationShard> GetRecentLocationAsync(long userId)
        {
			return await accounts.GetRecentLocationAsync(userId);
        }

        public async Task<HauntShard> GetUserHauntAsync(long userId)
        {
			return await accounts.GetUserHauntAsync(userId);
        }

        public async Task HardDeleteAsync(long userId)
        {
            await accounts.HardDeleteAsync(userId);
        }

        public async Task SoftDeleteAsync(long userId)
        {
            await accounts.SoftDeleteAsync(userId);
        }

        public async Task UpdateHauntAsync(long userId, double latitude, double longitude, double radius, int stability)
        {
			await accounts.UpdateHauntAsync(userId, latitude, longitude, radius, stability);
        }

        public async Task UpdateRecentLocationAsync(long userId, double latitude, double longitude, double radius)
        {
			await accounts.UpdateRecentLocationAsync(userId, latitude, longitude, radius);
        }

        public async Task UpdateUserAsync(long userId, List<(string Property, object Value)> edits)
        {
			await accounts.UpdateUserAsync(userId, edits);
        }
    }

	public class NotificationServiceStub : INotificationService
	{
		public static ConcurrentDictionary<string, ConcurrentBag<CanaryNotification>> messages = new();

        public async Task CancelNotification(string notificationId)
        {
            // no-op
            // TODO
        }

        public Task<string> DispatchNotification(CanaryNotification notification, params NotificationProfile[] notificationProfiles)
        {
            if (notificationProfiles.Length < 1)
            { return Task.FromResult(""); }

            ConcurrentBag<CanaryNotification> userBag;
            var notificationId = notificationProfiles[0].NotificationId.ToString();
            var exists = messages.TryGetValue(notificationId, out userBag);

            if (!exists)
            {
                userBag = new();
                messages.TryAdd(notificationId.ToString(), userBag);
            }

            userBag.Add(notification);
            return Task.FromResult("");
        }

        public Task<string> ScheduleNotification(CanaryNotification notification, DateTimeOffset dispatchAt, params NotificationProfile[] notificationProfiles)
        {
            if (notificationProfiles.Length < 1)
            { return Task.FromResult(""); }

            ConcurrentBag<CanaryNotification> userBag;
            var notificationId = notificationProfiles[0].NotificationId.ToString();
            var exists = messages.TryGetValue(notificationId, out userBag);

            if (!exists)
            {
                userBag = new();
                messages.TryAdd(notificationId.ToString(), userBag);
            }

            userBag.Add(notification);
            return Task.FromResult("");
        }
    }
}
