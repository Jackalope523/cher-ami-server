using Core.Boundaries;
using Core.Controls;
using Core.Entities;

using System;
using System.Threading.Tasks;
using Xunit;

namespace Core.Tests.Controls
{
    public class NestDirectorTests : CoreTest
    {
		private NestDirector director;

        public NestDirectorTests()
        {
			director = environment.Terminal.NestDirector;
        }

		[Fact]
		public async Task GetNestAsync_Self_ReturnsNest()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var host = await environment.GenerateUniqueUserAsync();

			var hostedGathering = await environment.GeneratePastGatheringAsync(user);
			var attendedGathering = await environment.GeneratePastGatheringAsync(host, user);
			var unattendedGathering = await environment.GeneratePastGatheringAsync(host);
			var ongoingGathering = await environment.GenerateUpcomingGatheringAsync(host, user);

			// Act
			var nest = await director.GetNestAsync(user.Id, user.Id);

			// Assert
			Assert.Equal(3, nest.Twigs.Count);
			Assert.Equal(hostedGathering.Id, nest.Twigs.Find(e => e.GatheringId.Equals(hostedGathering.Id)).GatheringId);
			Assert.Equal(attendedGathering.Id, nest.Twigs.Find(e => e.GatheringId.Equals(attendedGathering.Id)).GatheringId);
			Assert.Equal(ongoingGathering.Id, nest.Twigs.Find(e => e.GatheringId.Equals(ongoingGathering.Id)).GatheringId);
		}

		[Fact]
		public async Task GetNestAsync_Companion_ReturnsNest()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var companion = await environment.GenerateUniqueUserAsync();
			var host = await environment.GenerateUniqueUserAsync();
			await environment.ForceCompanionshipAsync(user, companion);

			var hostedGathering = await environment.GeneratePastGatheringAsync(user, companion);
			var mutuallyAttendedGathering = await environment.GeneratePastGatheringAsync(host, user, companion);
			var unattendedGathering = await environment.GeneratePastGatheringAsync(companion);
			var ongoingGathering = await environment.GenerateUpcomingGatheringAsync(host, companion);

			// Act
			var nest = await director.GetNestAsync(user.Id, companion.Id);

            // Assert
            Assert.Equal(4, nest.Twigs.Count);
		}

		[Fact]
		public async Task GetNestAsync_NeutralHost_ReturnsPublicNest()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var randomUser = await environment.GenerateUniqueUserAsync();

			var mutuallyAttendedGathering = await environment.GeneratePastGatheringAsync(user, randomUser);
			var unattendedGathering = await environment.GeneratePastGatheringAsync(randomUser);

			// Act
			var nest = await director.GetNestAsync(user.Id, randomUser.Id);

            // Assert
            Assert.Equal(2, nest.Twigs.Count);
		}

		[Fact]
		public async Task GetNestAsync_Blocked_Fails()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var enemy = await environment.GenerateUniqueUserAsync();
			await environment.ForceEnemiesAsync(user, enemy);

			// Act
			var nest = director.GetNestAsync(user.Id, enemy.Id);

			// Assert
			await Assert.ThrowsAnyAsync<HollowException>(async () => await nest);
		}

        [Fact]
		public async Task GetUserAgendaAsync_ReturnsAgenda()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var host = await environment.GenerateUniqueUserAsync();

			var pastGathering = await environment.GeneratePastGatheringAsync(user);
			var ongoingGathering = await environment.GenerateOngoingGatheringAsync(user);
			var upcomingGathering = await environment.GenerateUpcomingGatheringAsync(host, user);
			var anotherUpcomingGathering = await environment.GenerateUpcomingGatheringAsync(user);

			// Act
			var agenda = await director.GetUserAgendaAsync(user.Id);

			// Assert
			Assert.Equal(3, agenda.Cards.Count);
		}

		[Fact]
		public async Task GetCompanionAgendasAsync_ReturnsCompanionAgenda()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var activeCompanion = await environment.GenerateUniqueUserAsync();
			var sloadButChill = await environment.GenerateUniqueUserAsync();
			var randomHost = await environment.GenerateUniqueUserAsync();
			await environment.ForceCompanionshipAsync(user, activeCompanion, sloadButChill);

			var pastGathering = await environment.GeneratePastGatheringAsync(activeCompanion);
			var ongoingGathering = await environment.GenerateOngoingGatheringAsync(activeCompanion, sloadButChill);
			var upcomingGathering = await environment.GenerateUpcomingGatheringAsync(user, activeCompanion);
			var anotherUpcomingGathering = await environment.GenerateUpcomingGatheringAsync(randomHost, activeCompanion);
			var yetAnotherUpcomingGathering = await environment.GenerateUpcomingGatheringAsync(activeCompanion);

			// Act
			var agenda = await director.GetCompanionAgendasAsync(user.Id);

			// Assert
			Assert.Equal(2, agenda.Keys.Count);
			Assert.Equal(4, agenda[activeCompanion.Id].Cards.Count);
			Assert.Single(agenda[sloadButChill.Id].Cards);
		}

		[Fact]
		public async Task GetCompanionsAsync_ReturnsCompanions()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var companion = await environment.GenerateUniqueUserAsync();
			var otherCompanion = await environment.GenerateUniqueUserAsync();
			await environment.ForceCompanionshipAsync(user, companion, otherCompanion);

			// Act
			var companions = await director.GetCompanionsAsync(user.Id);

			// Assert
			Assert.Equal(2, companions.Count);

		}

		[Fact]
		public async Task GetFollowedUsersAsync_ReturnsUsers()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var companion = await environment.GenerateUniqueUserAsync();
			var otherCompanion = await environment.GenerateUniqueUserAsync();
			await environment.ForceCompanionshipAsync(user, companion, otherCompanion);

			// Act
			var followedUsers = await director.GetIncomingCompanionshipRequestsAsync(user.Id);

			// Assert
			Assert.Equal(2, followedUsers.Count);
		}

		[Fact]
		public async Task GetBlockedUsersAsync_ReturnsUsers()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var notACompanion = await environment.GenerateUniqueUserAsync();
			var archNemesis = await environment.GenerateUniqueUserAsync();
			await environment.ForceEnemiesAsync(user, notACompanion, archNemesis);

			// Act
			var blockedUsers = await director.GetBlockedUsersAsync(user.Id);

			// Assert
			Assert.Equal(2, blockedUsers.Count);
		}

		[Fact]
		public async Task FollowUserAsync_Succeeds()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var stranger = await environment.GenerateUniqueUserAsync();

			// Act
			await director.AcceptOrRequestCompanionshipAsync(user.Id, stranger.Id);
			// If no exception is thrown, the test is successful
		}

		[Fact]
		public async Task FollowUserAsync_Blocked_Fails()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var enemy = await environment.GenerateUniqueUserAsync();
			await environment.ForceEnemiesAsync(user, enemy);

			// Act
			var action = director.AcceptOrRequestCompanionshipAsync(user.Id, enemy.Id);

			// Assert
			await Assert.ThrowsAnyAsync<HollowException>(async () => await action);
		}

		[Fact]
		public async Task UnfollowUserAsync_FollowedUser_Succeeds()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var weirdDynamics = await environment.GenerateUniqueUserAsync();
			await director.AcceptOrRequestCompanionshipAsync(user.Id, weirdDynamics.Id);

			// Act
			await director.DenyOrRemoveUserAsync(user.Id, weirdDynamics.Id);
			// If no exception is thrown, the test is successful
		}

		[Fact]
		public async Task BlockUserAsync_Succeeds()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var strangerDanger = await environment.GenerateUniqueUserAsync();

			// Act
			await director.BlockUserAsync(user.Id, strangerDanger.Id);
			// If no exception is thrown, the test is successful
		}

		[Fact]
		public async Task UnblockUserAsync_Succeeds()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var weirdDynamics = await environment.GenerateUniqueUserAsync();
			await director.BlockUserAsync(user.Id, weirdDynamics.Id);

			// Act
			await director.UnblockUserAsync(user.Id, weirdDynamics.Id);
			// If no exception is thrown, the test is successful
		}
	}
}
