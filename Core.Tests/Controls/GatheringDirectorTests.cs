using Core.Boundaries;
using Core.Controls;
using Core.Entities;

using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Core.Tests.Controls
{
    public class GatheringDirectorTests : CoreTest
    {
		private GatheringDirector director;

        public GatheringDirectorTests()
        {
			director = environment.Terminal.GatheringDirector;
        }

		[Fact]
		public async Task GetGatheringInformationAsync_Host_ReturnsGathering()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			var returnedGathering = await director.GetGatheringInformationAsync(host.Id, gathering.Id);

			// Assert
			Assert.True(gathering.ToGatheringShard().Equals(returnedGathering));
		}

		[Fact]
		public async Task GetGatheringInformationAsync_Neutral_ReturnsGathering()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			var returnedGathering = await director.GetGatheringInformationAsync(user.Id, gathering.Id);

			// Assert
			Assert.True(gathering.ToGatheringShard().Equals(returnedGathering));
		}

		[Fact]
		public async Task GetGatheringInformationAsync_BlockedGathering_Fails()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();
			await environment.ForceEnemiesAsync(host, user);

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			var returnedGathering = director.GetGatheringInformationAsync(user.Id, gathering.Id);

			// Assert
			await Assert.ThrowsAnyAsync<HollowException>(async () => await returnedGathering);
		}

		[Fact]
		public async Task GetGatheringsInAreaAsync_ReturnsGathering()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUniqueGatheringAsync(host);

			// Act
			var nearbyGatherings = await director.GetGatheringsInAreaAsync(user.Id, gathering.Location.Latitude, gathering.Location.Longitude, 10);

			// Assert
			Assert.Single(nearbyGatherings);
		}

		[Fact]
		public async Task GetGatheringsInAreaAsync_MultipleGatherings_ReturnsGatherings()
		{
			// Arrange
			var host1 = await environment.GenerateUniqueUserAsync();
			var host2 = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();

			var gatherings = await environment.GenerateMultipleUniqueGatheringAsync(host1, host2);

			// Act
			var nearbyGatherings = await director.GetGatheringsInAreaAsync(user.Id, gatherings[0].Location.Latitude, gatherings[0].Location.Longitude, 10);

			// Assert
			Assert.Equal(2, nearbyGatherings.Count);
		}

		[Fact]
		public async Task GetGatheringsInAreaAsync_FarGathering_ReturnsCloseGathering()
		{
			// Arrange
			var host1 = await environment.GenerateUniqueUserAsync();
			var host2 = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();

			var closeGathering = await environment.GenerateUniqueGatheringAsync(host1);
			await environment.GenerateUniqueGatheringAsync(host2);

			// Act
			var nearbyGatherings = await director.GetGatheringsInAreaAsync(user.Id, closeGathering.Location.Latitude, closeGathering.Location.Longitude, 1);

			// Assert
			Assert.Single(nearbyGatherings);
		}

		[Fact]
		public async Task GetGatheringsInAreaAsync_BlockedGathering_ReturnsNothing()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();
			await environment.ForceEnemiesAsync(host, user);

			var gathering = await environment.GenerateUniqueGatheringAsync(host);
			
			// Act
			var nearbyGatherings = await director.GetGatheringsInAreaAsync(user.Id, gathering.Location.Latitude, gathering.Location.Longitude, 10);

			// Assert
			Assert.Empty(nearbyGatherings);
		}

		[Fact]
		public async Task GetGatheringsInAreaAsync_NoGatherings_ReturnsNothing()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();

			// Act
			var nearbyGatherings = await director.GetGatheringsInAreaAsync(user.Id, 90, 0, 1);

			// Assert
			Assert.Empty(nearbyGatherings);
		}

		[Fact]
		public async Task GetPersonalisedGatheringsInAreaAsync_ReturnsGatherings()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			var nearbyGatherings = await director.GetGatheringsInAreaAsync(user.Id, gathering.Location.Latitude, gathering.Location.Longitude, 10);

			// Assert
			Assert.Single(nearbyGatherings);
		}

		[Fact]
		public async Task CreateGatheringAsync_ValidGathering_ReturnsGathering()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var gatheringStub = environment.CreateTestGathering(host);
			gatheringStub.StartTime = new(DateTime.UtcNow + TimeSpan.FromDays(1));

			// Act
			var returnedGathering = await director.CreateGatheringAsync(host.Id,
				gatheringStub.Title, gatheringStub.Description,
				gatheringStub.StartTime,
                gatheringStub.Location.Latitude, gatheringStub.Location.Longitude,
				gatheringStub.FriendlyLocation,
                gatheringStub.Radius.Kilometres, gatheringStub.IsDynamic,
				gatheringStub.DegreeOfPrivacy,
                gatheringStub.GroupMinimum, gatheringStub.GroupMaximum,
                new System.IO.MemoryStream { });

            // Assert
            Assert.Equal(gatheringStub.Title, returnedGathering.Title);
			Assert.Equal(gatheringStub.Description, returnedGathering.Description);
			Assert.Equal(gatheringStub.StartTime, returnedGathering.StartTime);

			Assert.Equal(gatheringStub.Location.Latitude, returnedGathering.Latitude);
			Assert.Equal(gatheringStub.Location.Longitude, returnedGathering.Longitude);
			Assert.Equal(gatheringStub.Radius.Kilometres, returnedGathering.Radius);

			Assert.Equal(gatheringStub.GroupMinimum, returnedGathering.GroupMinimum);
			Assert.Equal(gatheringStub.GroupMaximum, returnedGathering.GroupMaximum);
		}

		[Fact]
		public async Task CreateGatheringAsync_InvalidGathering_Fails()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var gatheringStub = environment.CreateTestGathering(host);
			gatheringStub.StartTime = new(DateTime.UtcNow + TimeSpan.FromDays(30));
			gatheringStub.GroupMaximum = 3;
			gatheringStub.GroupMinimum = 5;

			// Act
			var returnedGathering = director.CreateGatheringAsync(host.Id,
				gatheringStub.Title, gatheringStub.Description,
				gatheringStub.StartTime,
				gatheringStub.Location.Latitude, gatheringStub.Location.Longitude,
				gatheringStub.FriendlyLocation,
				gatheringStub.Radius.Kilometres, gatheringStub.IsDynamic,
				gatheringStub.DegreeOfPrivacy,
				gatheringStub.GroupMinimum, gatheringStub.GroupMaximum,
				new System.IO.MemoryStream { });

			// Assert
			await Assert.ThrowsAnyAsync<HollowException>(async () => await returnedGathering);
		}

		[Fact]
		public async Task CreateGatheringAsync_UserCannotHost_Fails()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			await environment.UpdateUser(host, nameof(CoreUser.AccountStatus), UserAccountStatus.Impotent);

			var gatheringStub = environment.CreateTestGathering(host);

			// Act
			var returnedGathering = director.CreateGatheringAsync(host.Id,
				gatheringStub.Title, gatheringStub.Description,
				gatheringStub.StartTime,
                gatheringStub.Location.Latitude, gatheringStub.Location.Longitude,
				gatheringStub.FriendlyLocation,
                gatheringStub.Radius.Kilometres, gatheringStub.IsDynamic,
				gatheringStub.DegreeOfPrivacy,
                gatheringStub.GroupMinimum, gatheringStub.GroupMaximum,
                new System.IO.MemoryStream { });

            // Assert
            await Assert.ThrowsAnyAsync<HollowException>(async () => await returnedGathering);
		}

		[Fact]
		public async Task CreateGatheringAsync_GatheringConflict_Fails()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var gathering = environment.GenerateUpcomingGatheringAsync(host);
			var conflictingGathering = environment.CreateTestGathering(host);

			// Act
			var returnedGathering = director.CreateGatheringAsync(host.Id,
				conflictingGathering.Title, conflictingGathering.Description,
				conflictingGathering.StartTime,
				conflictingGathering.Location.Latitude, conflictingGathering.Location.Longitude,
				conflictingGathering.FriendlyLocation,
				conflictingGathering.Radius.Kilometres, conflictingGathering.IsDynamic,
				conflictingGathering.DegreeOfPrivacy,
				conflictingGathering.GroupMinimum, conflictingGathering.GroupMaximum,
				new System.IO.MemoryStream { });

			// Assert
			await Assert.ThrowsAnyAsync<HollowException>(async () => await returnedGathering);
		}

		[Fact]
		public async Task EditGatheringAsync_ValidInput_UpdatesGathering()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var gatheringStub = await environment.GenerateUpcomingGatheringAsync(host);
			
			string newDescription = "new description";
			DateTimeOffset newStartTime = new(DateTime.UtcNow + TimeSpan.FromDays(2));
			bool newIsOpen = false;

			double newLatitude = 50, newLongitude = 50, newRadius = 3.14;
			bool newIsDynamic = true;

			int newGroupMinimum = 7, newGroupMaximum = 41;

			// Act
			await director.EditGatheringAsync(host.Id, gatheringStub.Id,
				gatheringDescription: newDescription, startTime: newStartTime,
				latitude: newLatitude, longitude: newLongitude,
				radius: newRadius, isDynamic: newIsDynamic,
				groupMinimum: newGroupMinimum, groupMaximum: newGroupMaximum);

			// Assert
			var returnedGathering = await director.GetGatheringInformationAsync(host.Id, gatheringStub.Id);

			Assert.Equal(newDescription, returnedGathering.Description);
			Assert.Equal(newStartTime, returnedGathering.StartTime);
			// TODO Fix Assert.Equal(newIsOpen, new Gathering(returnedGathering).IsOpen);

			Assert.Equal(newLatitude, returnedGathering.Latitude);
			Assert.Equal(newLongitude, returnedGathering.Longitude);
			Assert.Equal(newRadius, returnedGathering.Radius);

			Assert.Equal(newGroupMinimum, returnedGathering.GroupMinimum);
			Assert.Equal(newGroupMaximum, returnedGathering.GroupMaximum);
		}

		[Fact]
		public async Task EndGatheringAsync_EndsGathering()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateOngoingGatheringAsync(user);

			// Act
			await director.TerminateGatheringAsync(user.Id, gathering.Id);
			// If no exception is thrown, the test is successful
		}

		[Fact]
		public async Task JoinGatheringAsync_ValidGathering_Succeeds()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);
            await environment.UpdateUserLocationAsync(user, gathering.Location.Latitude, gathering.Location.Longitude);

            // Act
            await director.JoinGatheringAsync(user.Id, gathering.Id);
			// If no exception is thrown, the test is successful
		}

		[Fact]
		public async Task JoinGatheringAsync_InvalidGathering_Fails()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GeneratePastGatheringAsync(host);
            await environment.UpdateUserLocationAsync(user, gathering.Location.Latitude, gathering.Location.Longitude);

            // Act
            var join = director.JoinGatheringAsync(user.Id, gathering.Id);

			// Assert
			await Assert.ThrowsAnyAsync<HollowException>(async () => await join);
		}

		[Fact]
		public async Task LeaveGatheringAsync_Succeeds()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);
            await environment.UpdateUserLocationAsync(user, gathering.Location.Latitude, gathering.Location.Longitude);
            await director.JoinGatheringAsync(user.Id, gathering.Id);

			// Act
			await director.LeaveGatheringAsync(user.Id, gathering.Id);
			// If no exception is thrown, the test is successful
		}

		[Fact]
		public async Task GetGuestListAsync_HostingGathering_ReturnsUsers()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var guest = await environment.GenerateUniqueUserAsync();
			var left = await environment.GenerateUniqueUserAsync();
			var incoming = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateOngoingGatheringAsync(user, guest);
			await environment.AddUserToGatheringAsync(gathering, left, GatheringBond.Left);
			await environment.AddUserToGatheringAsync(gathering, incoming, GatheringBond.Guest);

			// Act
			var guestList = await director.GetGuestListAsync(user.Id, gathering.Id);

			// Assert
			Assert.Equal(2, guestList.Count);

			Assert.Equal(2, guestList.Where(user => user.Bond.Equals(GatheringBond.Arrived)).Count());
			Assert.Single(guestList, user => user.Bond.Equals(GatheringBond.Left));
			Assert.Single(guestList, user => user.Bond.Equals(GatheringBond.Guest));
		}

		[Fact]
		public async Task GetGuestListAsync_GuestAtValidGathering_ReturnsGuests()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();
			var left = await environment.GenerateUniqueUserAsync();
			var incoming = await environment.GenerateUniqueUserAsync();
			var incomingCompanion = await environment.GenerateUniqueUserAsync();

            await environment.ForceCompanionshipAsync(user, incomingCompanion);

            var gathering = await environment.GenerateOngoingGatheringAsync(host, user);
			await environment.AddUserToGatheringAsync(gathering, left, GatheringBond.Left);
			await environment.AddUserToGatheringAsync(gathering, incoming, GatheringBond.Guest);
			await environment.AddUserToGatheringAsync(gathering, incomingCompanion, GatheringBond.Guest);

			// Act
			var guestList = await director.GetGuestListAsync(user.Id, gathering.Id);

			// Assert
			Assert.Equal(2, guestList.Count);

			Assert.Equal(2, guestList.Where(user => user.Bond.Equals(GatheringBond.Arrived)).Count());
			Assert.Single(guestList, user => user.Bond.Equals(GatheringBond.Left));
			Assert.Single(guestList, user => user.Bond.Equals(GatheringBond.Guest));
		}

		[Fact]
		public async Task GetGuestListAsync_ViewingValidGathering_ReturnsCompanions()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();
			var guestCompanion = await environment.GenerateUniqueUserAsync();
			var left = await environment.GenerateUniqueUserAsync();
			var leftCompanion = await environment.GenerateUniqueUserAsync();
			var incoming = await environment.GenerateUniqueUserAsync();
			var incomingCompanion = await environment.GenerateUniqueUserAsync();

			await environment.ForceCompanionshipAsync(user, guestCompanion, leftCompanion, incomingCompanion);

			var gathering = await environment.GenerateOngoingGatheringAsync(host, guestCompanion);
			await environment.AddUserToGatheringAsync(gathering, left, GatheringBond.Left);
			await environment.AddUserToGatheringAsync(gathering, leftCompanion, GatheringBond.Left);
			await environment.AddUserToGatheringAsync(gathering, incoming, GatheringBond.Guest);
			await environment.AddUserToGatheringAsync(gathering, incomingCompanion, GatheringBond.Guest);

			// Act
			var guestList = await director.GetGuestListAsync(user.Id, gathering.Id);

			// Assert
			Assert.Equal(2, guestList.Count);

			Assert.Single(guestList, user => user.Bond.Equals(GatheringBond.Arrived));
			Assert.Single(guestList, user => user.Bond.Equals(GatheringBond.Left));
			Assert.Single(guestList, user => user.Bond.Equals(GatheringBond.Guest));
		}

		[Fact]
		public async Task GetGuestListAsync_ViewingPastGathering_ReturnsCompanions()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();
			var companion = await environment.GenerateUniqueUserAsync();
			var stranger = await environment.GenerateUniqueUserAsync();

			await environment.ForceCompanionshipAsync(user, companion);

			var gathering = await environment.GeneratePastGatheringAsync(host, companion, stranger);

			// Act
			var guestList = await director.GetGuestListAsync(user.Id, gathering.Id);

			// Assert
			Assert.Equal(3, guestList.Count);

			Assert.Empty(guestList.Where(user => user.Bond.Equals(GatheringBond.Arrived)));
			Assert.Single(guestList.Where(user => user.Bond.Equals(GatheringBond.Left)));
			Assert.Empty(guestList.Where(user => user.Bond.Equals(GatheringBond.Guest)));
			//Assert.Empty(guestList.Where(user => user.Bond.Equals(GatheringBond.Watching)));
		}

		[Fact]
		public async Task GetGuestListAsync_InvalidGathering_Fails()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();
			await environment.ForceEnemiesAsync(user, host);

			var gathering = await environment.GenerateOngoingGatheringAsync(host);

			// Act
			var guestList = director.GetGuestListAsync(user.Id, gathering.Id);

			// Assert
			await Assert.ThrowsAnyAsync<HollowException>(async () => await guestList);
		}

		[Fact]
		public async Task GetPotentialInviteesAsync_ValidGathering_ReturnsUsers()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var host = await environment.GenerateUniqueUserAsync();
			var companion = await environment.GenerateUniqueUserAsync();
			await environment.ForceCompanionshipAsync(user, companion);

			var gathering = await environment.GenerateUpcomingGatheringAsync(host, user);
			await environment.UpdateUserLocationAsync(companion, gathering.Location.Latitude, gathering.Location.Longitude);

			// Act
			var invitees = await director.GetPotentialInviteesAsync(user.Id, gathering.Id);

			// Assert
			Assert.Single(invitees);
		}

		[Fact]
		public async Task InviteUserAsync_ValidCompanionValidGathering_Succeeds()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var host = await environment.GenerateUniqueUserAsync();
			var companion = await environment.GenerateUniqueUserAsync();
			await environment.ForceCompanionshipAsync(user, companion);

			var gathering = await environment.GenerateUpcomingGatheringAsync(host, user);
            await environment.UpdateUserLocationAsync(companion, gathering.Location.Latitude, gathering.Location.Longitude);

            // Act
            await director.InviteUserAsync(user.Id, companion.Id, gathering.Id);
			// If no exception is thrown, the test is successful
		}

		[Fact]
		public async Task InviteUserAsync_ValidCompanionInvalidGathering_Fails()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var host = await environment.GenerateUniqueUserAsync();
			var companion = await environment.GenerateUniqueUserAsync();
			await environment.ForceCompanionshipAsync(user, companion);

			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			var invite = director.InviteUserAsync(user.Id, companion.Id, gathering.Id);

			// Assert
			await Assert.ThrowsAnyAsync<HollowException>(async () => await invite);
		}

		[Fact]
		public async Task InviteUserAsync_InvalidCompanionValidGathering_Fails()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var stranger = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateUpcomingGatheringAsync(user);

			// Act
			var invite = director.InviteUserAsync(user.Id, stranger.Id, gathering.Id);

			// Assert
			await Assert.ThrowsAnyAsync<HollowException>(async () => await invite);
		}

		[Fact]
		public async Task KickUserAsync_HostedGathering_Succeeds()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			var stranger = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateOngoingGatheringAsync(user, stranger);

			// Act
			await director.KickUserAsync(user.Id, stranger.Id, gathering.Id);
			// If no exception is thrown, the test is successful
		}

		[Fact]
		public async Task KickUserAsync_NotHostingGathering_Fails()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var user = await environment.GenerateUniqueUserAsync();
			var stranger = await environment.GenerateUniqueUserAsync();

			var gathering = await environment.GenerateOngoingGatheringAsync(host, user, stranger);

			// Act
			var kick = director.KickUserAsync(user.Id, stranger.Id, gathering.Id);

			// Assert
			await Assert.ThrowsAnyAsync<HollowException>(async () => await kick);
		}

	}
}
