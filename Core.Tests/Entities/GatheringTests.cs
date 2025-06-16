using System;
using Core.Boundaries;
using System.Threading.Tasks;
using Core.Entities;
using Xunit;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;

using System.IO;
using System.Collections.Concurrent;

namespace Core.Tests.Entities
{
    public class GatheringTests : CoreTest
	{
		///////
		// Composition
		////////////////

		[Fact]
		public void ValidateAndNormalise_ValidGathering_ReturnsTrue()
		{
			// Arrange
			var validGathering = new Gathering
			{
				Title = "Valid Gathering",
				Description = "A valid gathering description",
				StartTime = DateTimeOffset.Now,
				GroupMinimum = 5,
				GroupMaximum = 10
			};

			// Act
			bool result = validGathering.ValidateAndNormalise(out string _);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void ValidateAndNormalise_InvalidGathering_ReturnsFalse()
		{
			// Arrange
			var invalidGathering = new Gathering
			{
				Title = "Invalid Gathering",
				Description = "A".PadLeft(Gathering.MaximumDescLength + 1),
				StartTime = DateTimeOffset.Now - TimeSpan.FromDays(8),
				GroupMinimum = 5,
				GroupMaximum = 2
			};

			// Act
			bool result = invalidGathering.ValidateAndNormalise(out string _);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task GetCompanionsOf_ReturnsCompanions()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();
			var companion1 = await environment.GenerateUniqueUserAsync();
			var companion2 = await environment.GenerateUniqueUserAsync();
			var randomGuest = await environment.GenerateUniqueUserAsync();
			await environment.ForceCompanionshipAsync(user, companion1, companion2);

			var gathering = await environment.GenerateUpcomingGatheringAsync(host, user, companion1, companion2, randomGuest);

			// Act
			var companions = await gathering.GetCompanionsOf(companion2);

			// Assert
			Assert.Equal(2, companions.Count);
		}

		/////
		// Checks
		///////////

		[Fact]
		public async Task IsVisibleTo_Neutral_ReturnsTrue()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();
			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			var result = await gathering.IsVisibleTo(user);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public async Task IsVisibleTo_Blocked_ReturnsFalse()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();
			await environment.ForceEnemiesAsync(host, user);

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			var result = await gathering.IsVisibleTo(user);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task IsJoinableTo_Neutral_ReturnsTrue()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();
			var gathering = await environment.GenerateUpcomingGatheringAsync(host);
			await environment.UpdateUserLocationAsync(user, gathering.Location.Latitude, gathering.Location.Longitude);

			// Act
			var result = await gathering.IsJoinableBy(user);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public async Task IsJoinableTo_Blocked_ReturnsFalse()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();
			await environment.ForceEnemiesAsync(host, user);

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			var result = await gathering.IsJoinableBy(user);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task IsModifiableBy_Host_ReturnsTrue()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			var result = gathering.IsModifiableBy(host);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public async Task IsModifiableBy_Guest_ReturnsFalse()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var guest = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host, guest);

			// Act
			var result = gathering.IsModifiableBy(guest);

			// Assert
			Assert.False(result);
			
		}

		[Fact]
		public async Task IsModifiableBy_Neutral_ReturnsFalse()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			var result = gathering.IsModifiableBy(user);

			// Assert
			Assert.False(result);
			
		}

		[Fact]
		public async Task IsHostedBy_Host_ReturnsTrue()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			var result = gathering.IsHostedBy(host);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public async Task IsHostedBy_Guest_ReturnsFalse()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var guest = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host, guest);

			// Act
			var result = gathering.IsHostedBy(guest);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task IsHostedBy_Neutral_ReturnsFalse()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			var result = gathering.IsHostedBy(user);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task HasUserRelationship_Host_ReturnsTrue()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			var result = await gathering.HasUserRelationship(host);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public async Task HasUserRelationship_Incoming_ReturnsTrue()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var incomingGuest = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);
			await environment.AddUserToGatheringAsync(gathering, incomingGuest, GatheringBond.Guest);

			// Act
			var result = await gathering.HasUserRelationship(incomingGuest);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public async Task HasUserRelationship_ActiveGuest_ReturnsTrue()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var guest = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);
			await environment.AddUserToGatheringAsync(gathering, guest, GatheringBond.Arrived);

			// Act
			var result = await gathering.HasUserRelationship(guest);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public async Task HasUserRelationship_LeftGuest_ReturnsTrue()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var leftGuest = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);
			await environment.AddUserToGatheringAsync(gathering, leftGuest, GatheringBond.Left);

			// Act
			var result = await gathering.HasUserRelationship(leftGuest);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public async Task HasUserRelationship_KickedGuest_ReturnsTrue()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var kickedGuest = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);
			await environment.AddUserToGatheringAsync(gathering, kickedGuest, GatheringBond.Kicked);

			// Act
			var result = await gathering.HasUserRelationship(kickedGuest);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public async Task HasUserRelationship_Neutral_ReturnsFalse()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			var result = await gathering.HasUserRelationship(user);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task WasAttendedBy_ActiveGuest_ReturnsTrue()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var guest = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);
			await environment.AddUserToGatheringAsync(gathering, guest, GatheringBond.Arrived);

			// Act
			var result = await gathering.WasAttendedBy(guest);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public async Task WasAttendedBy_LeftGuest_ReturnsTrue()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var leftGuest = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);
			await environment.AddUserToGatheringAsync(gathering, leftGuest, GatheringBond.Left);

			// Act
			var result = await gathering.WasAttendedBy(leftGuest);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public async Task WasAttendedBy_KickedGuest_ReturnsFalse()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var kickedGuest = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);
			await environment.AddUserToGatheringAsync(gathering, kickedGuest, GatheringBond.Kicked);

			// Act
			var result = await gathering.WasAttendedBy(kickedGuest);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task WasAttendedBy_Neutral_ReturnsFalse()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			var result = await gathering.WasAttendedBy(user);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task IsInRange_CloseUser_ReturnsTrue()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);
			await environment.UpdateUserLocationAsync(user,
				gathering.Location.Latitude,
				gathering.Location.Longitude);

			// Act
			var result = await gathering.IsInRange(user);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public async Task IsInRange_FarUser_ReturnsFalse()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);
			await environment.UpdateUserLocationAsync(user,
				gathering.Location.Latitude + 15,
				gathering.Location.Longitude + 15);

			// Act
			var result = await gathering.IsInRange(user);

			// Assert
			Assert.False(result);
		}

		//////
		// Effects
		////////////

		[Fact]
		public async Task Ended_Succeeds()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			await gathering.Ended();
			// If no exception is thrown, the test is successful
		}

		[Fact]
		public async Task Reported_Succeeds()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			await gathering.Reported();
			// If no exception is thrown, the test is successful
		}
	}
}
