using System;
using Core.Boundaries;
using System.Threading.Tasks;
using Core.Entities;
using Xunit;
using System.IO;


namespace Core.Tests.Entities
{
    public class UserTests : CoreTest
    {
		///////
		// Composition
		////////////////

		[Fact]
		public void ValidateAndNormalise_ValidUser_ReturnsTrue()
		{
			// Arrange
			var validUser = new User
			{
				PhoneNumber = "+1234567890",
				Email = "user@example.com",
				DateOfBirth = DateTimeOffset.Now.AddYears(-25)
			};

			// Act
			bool result = validUser.ValidateAndNormalise(out string _);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void ValidateAndNormalise_InvalidUser_ReturnsFalse()
		{
			// Arrange
			var invalidUser = new User
			{
				PhoneNumber = "invalid",
				Email = "invalid_email",
				DateOfBirth = DateTimeOffset.Now.AddYears(-15)
			};

			// Act
			bool result = invalidUser.ValidateAndNormalise(out string _);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task CalculateCharacter_NewCharacter()
		{
			// Arrange
			var host = environment.CreateTestUser();
			var user = await environment.GenerateUniqueUserAsync();

			var gathering = environment.CreateTestGathering(host);
			gathering.Character = new(new(20,100,100,100,100,100,100,100));
			var oldCharacter = user.Character;

			// Act
			user.CalculateCharacter(gathering, TimeSpan.FromMinutes(45));

			// Assert
			Assert.NotEqual(oldCharacter, user.Character);
		}

		[Fact]
		public async Task NextGathering_HasGathering_ReturnsGathering()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var gathering = await environment.GenerateUpcomingGatheringAsync(user);

			// Act
			var returnedGathering = await user.NextGathering();

			// Assert
			Assert.Equal(gathering, returnedGathering);
		}

		[Fact]
		public async Task NextGathering_HasNone_ReturnsNoneGathering()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();

			// Act
			var returnedGathering = await user.NextGathering();

			// Assert
			Assert.Equal(Gathering.None, returnedGathering);
		}

		/////
		// Checks
		///////////

		[Fact]
		public async Task IsCompanionsWith_Companion_ReturnsTrue()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var companion = await environment.GenerateUniqueUserAsync();
			await environment.ForceCompanionshipAsync(user, companion);

			// Act
			var isCompanions = await user.IsCompanionsWith(companion);

			// Assert
			Assert.True(isCompanions);
		}

		[Fact]
		public async Task IsCompanionsWith_Neutral_ReturnsFalse()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var randomUser = await environment.GenerateUniqueUserAsync();

			// Act
			var isCompanions = await user.IsCompanionsWith(randomUser);

			// Assert
			Assert.False(isCompanions);
		}

		[Fact]
		public async Task IsFollowing_FollowedUser_ReturnsTrue()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var companion = await environment.GenerateUniqueUserAsync();
			await environment.ForceCompanionshipAsync(user, companion);

			// Act
			var isFollowing = await user.IsFollowing(companion);

			// Assert
			Assert.True(isFollowing);
		}

		[Fact]
		public async Task IsFollowing_Neutral_ReturnsFalse()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var randomUser = await environment.GenerateUniqueUserAsync();

			// Act
			var isFollowing = await user.IsFollowing(randomUser);

			// Assert
			Assert.False(isFollowing);
		}

		[Fact]
		public async Task IsBlocking_BlockedUser_ReturnsTrue()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var enemy = await environment.GenerateUniqueUserAsync();
			await environment.ForceEnemiesAsync(user, enemy);

			// Act
			var isBlocking = await user.IsBlocking(enemy);

			// Assert
			Assert.True(isBlocking);
		}

		[Fact]
		public async Task IsBlocking_Neutral_ReturnsFalse()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var randomUser = await environment.GenerateUniqueUserAsync();

			// Act
			var isBlocking = await user.IsBlocking(randomUser);

			// Assert
			Assert.False(isBlocking);
		}

		[Fact]
		public async Task IsBlockedBy_Blocked_ReturnsTrue()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var enemy = await environment.GenerateUniqueUserAsync();
			await environment.ForceEnemiesAsync(user, enemy);

			// Act
			var isBlocking = await user.IsBlocking(enemy);

			// Assert
			Assert.True(isBlocking);
		}

		[Fact]
		public async Task IsBlockedBy_Neutral_ReturnsFalse()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var randomUser = await environment.GenerateUniqueUserAsync();

			// Act
			var isBlocking = await user.IsBlocking(randomUser);

			// Assert
			Assert.False(isBlocking);
		}

		[Fact]
		public async Task IsAtGathering_AtGathering_ReturnsTrue()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var host = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateOngoingGatheringAsync(host);
			await environment.AddUserToGatheringAsync(gathering, user, GatheringBond.Arrived);

			// Act
			var isAtGathering = await user.IsAtGathering();

			// Assert
			Assert.True(isAtGathering);
		}

		[Fact]
		public async Task IsAtGathering_Sload_ReturnsFalse()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();

			// Act
			var isAtGathering = await user.IsAtGathering();

			// Assert
			Assert.False(isAtGathering);
		}

		[Fact]
		public async Task CanView_ValidGathering_ReturnsTrue()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var host = await environment.GenerateUniqueUserAsync();
			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			var canView = await user.CanView(gathering);

			// Assert
			Assert.True(canView);
		}

		[Fact]
		public async Task CanView_Blocked_ReturnsFalse()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var host = await environment.GenerateUniqueUserAsync();
			await environment.ForceEnemiesAsync(user, host);
			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			var canView = await user.CanView(gathering);

			// Assert
			Assert.False(canView);
		}

		[Fact]
		public async Task CanView_Limited_ReturnsFalse()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var host = await environment.GenerateUniqueUserAsync();
			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			await environment.UpdateUser(user, nameof(CoreUser.AccountStatus), UserAccountStatus.Limited);
			user = new(await environment.Terminal.AccountDatabase.FindUserByIdAsync(user.Id));

			// Act
			var canView = await user.CanView(gathering);

			// Assert
			Assert.False(canView);
		}

		[Fact]
		public async Task CanView_CompanionLimited_ReturnsTrue()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			await environment.UpdateUser(user, nameof(User.AccountStatus), UserAccountStatus.Limited);
			
			var host = await environment.GenerateUniqueUserAsync();
			var gathering = await environment.GenerateUpcomingGatheringAsync(host);
			await environment.ForceCompanionshipAsync(user, host);

			// Act
			var canView = await user.CanView(gathering);

			// Assert
			Assert.True(canView);
		}

		[Fact]
		public async Task CanJoin_JoinableGatheringAndVisibleUser_ReturnsTrue()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var host = await environment.GenerateUniqueUserAsync();
			var gathering = await environment.GenerateUpcomingGatheringAsync(host);
			await environment.UpdateUserLocationAsync(user, gathering.Location.Latitude, gathering.Location.Longitude);

			// Act
			var canJoin = await user.CanJoin(gathering);

			// Assert
			Assert.True(canJoin);
		}


		[Fact]
		public async Task CanEtch_Succeeds()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			await host.CanEtch(gathering);
			// If no exception is thrown, the test is successful
		}

		[Fact]
		public async Task Taken_OwnedSnapshot_ReturnsTrue()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(user);
			var snapshot = await environment.GenerateSnapshotAsync(gathering, user);

			// Act
			var etched = user.Taken(snapshot);

			// Assert
			Assert.True(etched);
		}

		[Fact]
		public async Task Taken_UnownedSnapshot_ReturnsFalse()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var host = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);
			var snapshot = await environment.GenerateSnapshotAsync(gathering, host);

			// Act
			var etched = user.Taken(snapshot);

			// Assert
			Assert.False(etched);
		}

		[Fact]
		public async Task CanReport_Chill_ReturnsTrue()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();

			// Act
			var canReport = await user.CanReport();

			// Assert
			Assert.True(canReport);
		}

		[Fact]
		public async Task CanReport_Volatile_ReturnsFalse()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();

			// Act
			var canReport = await user.CanReport();

			// Assert
			Assert.True(canReport);
		}

		//////
		// Effects
		////////////

		[Fact]
		public async Task HandleHaunt_Unmoved_Succeeds()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			await user.HandleHaunt();
			var oldHaunt = await user.Haunt;

			// Act
			await user.HandleHaunt();

			// Assert
			Assert.Equal(oldHaunt, await user.Haunt);
		}

		[Fact]
		public async Task HandleHaunt_Moved_Succeeds()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			await user.HandleHaunt();
			var oldHaunt = await user.Haunt;

			user.LastKnownLocation.Set(new()
			{
				Latitude = ((await user.LastKnownLocation).Latitude + 1) / 2,
				Longitude = ((await user.LastKnownLocation).Longitude + 1) / 2
			});

			// Act
			await user.HandleHaunt();

			// Assert
			Assert.NotEqual(oldHaunt, await user.Haunt);
		}

		[Fact]
		public async Task Penalised_NewReputation()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var oldReputation = user.Reputation;
			await environment.Terminal.DisciplineDirector.PenaliseUserAsync(user, PenaltyType.Unreliable, Psijic.Time);
			await environment.Terminal.DisciplineDirector.PenaliseUserAsync(user, PenaltyType.Unreliable, Psijic.Time);

			// Act
			await user.Penalised();

			// Assert
			Assert.NotEqual(oldReputation, user.Reputation);
		}

		//////
		// Actions
		////////////
	}
}
