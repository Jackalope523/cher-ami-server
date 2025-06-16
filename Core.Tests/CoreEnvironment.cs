using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
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
using PhoneNumbers;
using System.Timers;
using Serilog;
using Microsoft.Extensions.Logging;
using Core.Notifications;

namespace Core.Tests
{
    public class CoreEnvironment
	{
		public CoreTerminal Terminal;

		private int instance;

		private static ConcurrentBag<long> generatedUserIds = new();
		private long uniqueUserIncrement = 0;
		private string testUserPhoneNumberFormat = "000-{0}-{1}";
		private string testUserEmailFormat = "email_{0}_{1}@test.com";
		private string testUserName = "name";
		private DateTimeOffset subjectDateOfBirth = new(new DateTime(0));
		private DateTimeOffset testJoinDate = new(new DateTime(0));

		private static double uniqueGatheringDegree = -89;
		private string testGatheringName = "gathering1";
		private string testGatheringDescription = "The first of many.";
		private DateTimeOffset testGatheringStartTime = new(DateTime.UtcNow + TimeSpan.FromDays(1));
		private GeoLocation testGatheringLocation = new() { Latitude = 0, Longitude = 0 };
		private string testGatheringFriendlyLocation = "Somewhere, over the rainbow.";
		private int testGatheringGroupMinimum = 0;
		private int testGatheringGroupMaximum = 10;
		private Distance testGatheringRadius = new() { Kilometres = 1 };
		private bool testGatheringIsDynamic = false;

		private DateTimeOffset testSnapshotTime = new(DateTime.UtcNow);

		/////
		// Set-up
		///////////

		public CoreEnvironment(int instanceNumber)
		{
			instance = instanceNumber;

            // Arrange Core
			Repository.Harbor harbor = new(Repository.Harbor.Flag.Development);

			
            Terminal = CoreTerminal.CreateTerminal(
				new() { Flag = EnvironmentFlag.Development },
				new LoggerFactory().CreateLogger(""),
                new UserHook(harbor.AccountDatabaseAccess, generatedUserIds),
				harbor.AdminDatabaseAccess,
				harbor.ConnectionDatabaseAccess,
                harbor.GatheringDatabaseAccess,
                harbor.SnapshotDatabaseAccess,
                harbor.ReportDatabaseAccess,
				harbor.KeyDatabaseAccess,
                harbor.MediaDatabaseAccess,
				harbor.MessageDatabaseAccess,
                harbor.NotificationDatabaseAccess,
                harbor.NestDatabaseAccess,
				harbor.MiscellaneousDatabaseAccess,
				new NotificationServiceStub(),
				null);
			
		}

		///////
		// User Helpers
		/////////////////

		internal User CreateTestUser()
		{
			var userIncrement = Interlocked.Increment(ref uniqueUserIncrement);

			User userStub = new()
			{
				PhoneNumber = string.Format(testUserPhoneNumberFormat, instance.ToString("D3"), userIncrement.ToString("D4")),
				Email = string.Format(testUserEmailFormat, instance.ToString("D3"), userIncrement.ToString("D4")),
				Name = testUserName,
				DateOfBirth = subjectDateOfBirth,
				JoinDate = testJoinDate
			};

			return userStub;
		}

		internal async Task<User> GenerateUserUnsafeAsync(User userStub)
        {
            await Terminal.AccountDatabase.CreateUserAsync(userStub.PhoneNumber, userStub.Email, userStub.Email,
				userStub.Name, userStub.DateOfBirth, DateTimeOffset.UtcNow,
				CharacterVector.Default(userStub.GetAge()).ToCharacter(), userStub.NotificationId);

			var user = await Terminal.AccountDatabase.FindUserByPhoneNumberAsync(userStub.PhoneNumber);

			return new(user);
		}

		internal async Task<User> GenerateUniqueUserAsync()
		{
			var userStub = CreateTestUser();

			return await GenerateUserUnsafeAsync(userStub);
		}

		internal async Task UpdateUser(User user, string property, object value)
		{
			await Terminal.AccountDatabase.UpdateUserAsync(user.Id, new() { (property, value) });
		}

		internal async Task UpdateUserLocationAsync(User user, double latitude, double longitude, double radius = 1)
		{
			await Terminal.AccountDatabase.UpdateRecentLocationAsync(user.Id, latitude, longitude, radius);
		}

		internal async Task ForceCompanionshipAsync(params User[] users)
		{
			foreach (var user in users)
			{
				foreach (var otherUser in users)
				{
					if (user.Equals(otherUser))
					{ continue; }

					await Terminal.NestDatabase.FollowUserAsync(user.Id, otherUser.Id, DateTimeOffset.UtcNow);
				}
			}
		}

		internal async Task ForceEnemiesAsync(params User[] users)
		{
			foreach (var user in users)
			{
				foreach (var otherUser in users)
				{
					if (user.Equals(otherUser))
					{ continue; }

					await Terminal.NestDatabase.BlockUserAsync(user.Id, otherUser.Id, DateTimeOffset.UtcNow);
				}
			}
		}

		////////
		// Gathering Helpers
		//////////////////

		internal Gathering CreateTestGathering(User host)
		{
			Gathering gatheringStub = new()
			{
				Title = testGatheringName,
				Description = testGatheringDescription,
				HostId = host.Id,
				StartTime = testGatheringStartTime,
				Location = testGatheringLocation,
				FriendlyLocation = testGatheringFriendlyLocation,
				GroupMinimum = testGatheringGroupMinimum,
				GroupMaximum = testGatheringGroupMaximum,
				Radius = testGatheringRadius,
				IsDynamic = testGatheringIsDynamic,
				DegreeOfPrivacy = 3,
			};

			gatheringStub.Host.Set(host);

			return gatheringStub;
		}

		internal async Task<Gathering> GenerateGatheringUnsafeAsync(Gathering gatheringStub, User host)
		{
			return new(await Terminal.GatheringDatabase.CreateGatheringAsync(host.Id, gatheringStub.Title, gatheringStub.Description,
				gatheringStub.StartTime, gatheringStub.Location.Latitude, gatheringStub.Location.Longitude, gatheringStub.FriendlyLocation,
				gatheringStub.GroupMinimum, gatheringStub.GroupMaximum, host.Character.ToCharacter(),
				gatheringStub.Radius.Kilometres, gatheringStub.IsDynamic, gatheringStub.DegreeOfPrivacy, gatheringStub.StartTime));
		}

		internal async Task<Gathering> GenerateUpcomingGatheringAsync(User host)
		{
			var gatheringStub = CreateTestGathering(host);

			return await GenerateGatheringUnsafeAsync(gatheringStub, host);
		}

		internal async Task<Gathering> GenerateUpcomingGatheringAsync(User host, params User[] guests)
		{
			var gatheringStub = await GenerateUpcomingGatheringAsync(host);

			foreach (var guest in guests)
            {
				await Terminal.GatheringDatabase.SetUserStateAsync(guest.Id, gatheringStub.Id, GatheringBond.Guest, DateTimeOffset.UtcNow);
			}

			return gatheringStub;
		}

		internal async Task<Gathering> GenerateOngoingGatheringAsync(User host, params User[] guests)
		{
			var gatheringStub = CreateTestGathering(host);
			gatheringStub.StartTime = DateTime.Now - TimeSpan.FromHours(2);

			gatheringStub = await GenerateGatheringUnsafeAsync(gatheringStub, host);
			await Terminal.GatheringDatabase.SetUserStateAsync(host.Id, gatheringStub.Id, GatheringBond.Arrived, DateTimeOffset.UtcNow);

			foreach (var guest in guests)
			{
				await Terminal.GatheringDatabase.SetUserStateAsync(guest.Id, gatheringStub.Id, GatheringBond.Arrived, DateTimeOffset.UtcNow);
			}

			return gatheringStub;
		}

		internal async Task<Gathering> GeneratePastGatheringAsync(User host, params User[] guests)
		{
			var gatheringStub = CreateTestGathering(host);
			gatheringStub.StartTime = DateTime.Now - TimeSpan.FromHours(2);

			gatheringStub = await GenerateGatheringUnsafeAsync(gatheringStub, host);
			await Terminal.GatheringDatabase.SetUserStateAsync(host.Id, gatheringStub.Id, GatheringBond.Arrived, DateTimeOffset.UtcNow);

			foreach (var guest in guests)
			{
				await Terminal.GatheringDatabase.SetUserStateAsync(guest.Id, gatheringStub.Id, GatheringBond.Arrived, DateTimeOffset.UtcNow);
			}

			await Terminal.GatheringDatabase.TerminateGatheringAsync(gatheringStub.Id, DateTimeOffset.UtcNow);

			return gatheringStub;
		}

		internal async Task<Gathering> GenerateUniqueGatheringAsync(User host, params User[] guests)
		{
			var gatheringStub = CreateTestGathering(host);
			gatheringStub.Location = new() { Latitude = SafeAdd(ref uniqueGatheringDegree, 1), Longitude = SafeAdd(ref uniqueGatheringDegree, 1) };

			if (uniqueGatheringDegree > 80)
			{ uniqueGatheringDegree = -89.5; }

			gatheringStub = await GenerateGatheringUnsafeAsync(gatheringStub, host);

			foreach (var guest in guests)
			{
				await Terminal.GatheringDatabase.SetUserStateAsync(guest.Id, gatheringStub.Id, GatheringBond.Arrived, DateTimeOffset.UtcNow);
			}

			return gatheringStub;
		}

		internal async Task<List<Gathering>> GenerateMultipleUniqueGatheringAsync(params User[] hosts)
		{
			double currentDegree = SafeAdd(ref uniqueGatheringDegree, 1);
			GeoLocation location = new() { Latitude = currentDegree, Longitude = currentDegree };

			if (uniqueGatheringDegree > 80)
			{ uniqueGatheringDegree = -89.5; }

			List<Gathering> gatherings = new();

			foreach (var host in hosts)
			{
				var gatheringStub = CreateTestGathering(host);
				gatheringStub.Location = location;

				gatherings.Add(await GenerateGatheringUnsafeAsync(gatheringStub, host));
			}

			return gatherings;
		}

		internal async Task AddUserToGatheringAsync(Gathering gathering, User user, GatheringBond state)
		{
			await Terminal.GatheringDatabase.SetUserStateAsync(user.Id, gathering.Id, state, DateTimeOffset.UtcNow);
		}

		internal async Task SetGatheringState(Gathering gathering, GatheringState state)
		{
			await Terminal.GatheringDatabase.UpdateGatheringAsync(gathering.Id, new() { (nameof(Gathering.State), state) });
			gathering.State = state;
		}

		/////////
		// Snapshot Helpers
		////////////////////

		internal async Task<SnapshotShard> GenerateSnapshotAsync(Gathering etchedGathering, User etcher)
		{
			return await GenerateSnapshotUnsafeAsync(etchedGathering, etcher, testSnapshotTime);
		}

		internal async Task<SnapshotShard> GenerateSnapshotUnsafeAsync(Gathering etchedGathering, User etcher, SnapshotShard snapshot)
		{
			return await Terminal.SnapshotDatabase.AddSnapshotAsync(etchedGathering.Id, etcher.Id, snapshot.TimeTaken);
		}

		internal async Task<SnapshotShard> GenerateSnapshotUnsafeAsync(Gathering etchedGathering, User etcher, DateTimeOffset timeTaken)
		{
			return await Terminal.SnapshotDatabase.AddSnapshotAsync(etchedGathering.Id, etcher.Id, timeTaken);
		}

		///////////
		// Notification Helpers
		/////////////////////////

		internal List<CanaryNotification> GetUserMessages(User user)
		{
			ConcurrentBag<CanaryNotification> userMessages;
			var exists = NotificationServiceStub.messages.TryGetValue(user.Id.ToString(), out userMessages);
			return exists ? userMessages.ToList() : new();
		}

		//////
		// Clean-up
		/////////////

		public async void DisposeEnvironment()
		{
			// Delete all users generated in this environment
            foreach (long id in generatedUserIds)
			{
				try
				{
					await Terminal.AdminDatabase.VoidUserAsync(id);
				}
				catch (Exception e) { Console.WriteLine(e); }
			}
		}

        public static double SafeAdd(ref double location1, double value)
        {
            double newCurrentValue = location1;
            while (true)
            {
                double currentValue = newCurrentValue;
                double newValue = currentValue + value;
                newCurrentValue = Interlocked.CompareExchange(ref location1, newValue, currentValue);
                if (newCurrentValue.Equals(currentValue))
                    return newValue;
            }
        }
    }
}
