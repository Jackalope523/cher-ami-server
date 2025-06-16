using Core.Boundaries;
using Core.Controls;
using Core.Entities;

using System;
using System.Threading.Tasks;
using Xunit;

namespace Core.Tests.Controls
{
    public class SnapshotDirectorTests : CoreTest
    {
		private SnapshotDirector director;

        public SnapshotDirectorTests()
        {
			director = environment.Terminal.SnapshotDirector;
        }

		[Fact]
		public async Task GetGatheringSnapshotsAsync_HostedGathering_ReturnsSnapshots()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var gathering = await environment.GenerateUpcomingGatheringAsync(host);
			await environment.GenerateSnapshotAsync(gathering, host);
			await environment.GenerateSnapshotAsync(gathering, host);

			// Act
			var serverSnapshots = await director.GetGalleryAsync(host.Id, gathering.Id);

			// Assert
			Assert.Equal(2, serverSnapshots.Snapshots.Count);
		}

		[Fact]
		public async Task GetGatheringSnapshotsAsync_GuestAtGathering_ReturnsSnapshots()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var guest = await environment.GenerateUniqueUserAsync();
			var gathering = await environment.GenerateOngoingGatheringAsync(host, guest);
			await environment.GenerateSnapshotAsync(gathering, host);
			await environment.GenerateSnapshotAsync(gathering, host);

			// Act
			var serverSnapshots = await director.GetGalleryAsync(guest.Id, gathering.Id);

			// Assert
			Assert.Equal(2, serverSnapshots.Snapshots.Count);
		}

		[Fact]
		public async Task GetGatheringSnapshotsAsync_InvalidGathering_Fails()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var sneakyUser = await environment.GenerateUniqueUserAsync();
			var gathering = await environment.GenerateUpcomingGatheringAsync(host);
			await environment.GenerateSnapshotAsync(gathering, host);
			await environment.GenerateSnapshotAsync(gathering, host);

			// Act
			var serverSnapshots = director.GetGalleryAsync(sneakyUser.Id, gathering.Id);

			// Assert
			await Assert.ThrowsAnyAsync<HollowException>(async () => await serverSnapshots);
		}

		[Fact]
		public async Task AddSnapshotAsync_GuestAtGathering_Succeeds()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var guest = await environment.GenerateUniqueUserAsync();
			var gathering = await environment.GenerateOngoingGatheringAsync(host, guest);
			byte[] image = { byte.MinValue, 0, 1, 3, byte.MaxValue, 7, 8 };

			// Act
			await director.AddSnapshotAsync(guest.Id, gathering.Id, new(image));

			// Assert
			var serverSnapshots = await director.GetGalleryAsync(guest.Id, gathering.Id);
			Assert.Single(serverSnapshots.Snapshots);

			var snapshot = serverSnapshots.Snapshots[0];
			Assert.Equal(image, (await environment.Terminal.MediaDatabase.DownloadSnapshotAsync(snapshot.Id, guest.Id)).ToArray());
		}

        [Fact]
        public async Task GetNestGalleryAsync_Self_ReturnsGallery()
        {
            // Arrange
            var user = await environment.GenerateUniqueUserAsync();
            var host = await environment.GenerateUniqueUserAsync();

            var hostedGathering = await environment.GeneratePastGatheringAsync(user);
            var attendedGathering = await environment.GeneratePastGatheringAsync(host, user);
            var unattendedGathering = await environment.GeneratePastGatheringAsync(host);
            var ongoingGathering = await environment.GenerateUpcomingGatheringAsync(host, user);

            var funLovingSnapshot = await environment.GenerateSnapshotAsync(attendedGathering, user);
            var lessLovingSnapshot = await environment.GenerateSnapshotAsync(attendedGathering, host);
            var okSnapshot = await environment.GenerateSnapshotAsync(ongoingGathering, user);

            // Act
            var gallery = await director.GetGalleryAsync(user.Id, attendedGathering.Id);

            // Assert
            Assert.Equal(2, gallery.Snapshots.Count);
            Assert.Equal(funLovingSnapshot, gallery.Snapshots.Find(e => e.Id.Equals(funLovingSnapshot.Id)));
            Assert.Equal(lessLovingSnapshot, gallery.Snapshots.Find(e => e.Id.Equals(lessLovingSnapshot.Id)));
        }

        [Fact]
        public async Task GetNestGalleryAsync_Companion_ReturnsGallery()
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

            var userSnapshot = await environment.GenerateSnapshotAsync(mutuallyAttendedGathering, user);
            var companionSnapshot = await environment.GenerateSnapshotAsync(mutuallyAttendedGathering, companion);
            var hostSnapshot = await environment.GenerateSnapshotAsync(mutuallyAttendedGathering, host);
            var yetAnotherCompanionSnapshot = await environment.GenerateSnapshotAsync(mutuallyAttendedGathering, companion);
            var unattendedGatheringCompanionSnapshot = await environment.GenerateSnapshotAsync(unattendedGathering, companion);

            // Act
            var gallery = await director.GetGalleryAsync(user.Id, mutuallyAttendedGathering.Id);

            // Assert
            Assert.Equal(2, gallery.Snapshots.Count);
        }

        [Fact]
        public async Task GetGalleryAsync_NeutralHost_ReturnsNothing()
        {
            // Arrange
            var user = await environment.GenerateUniqueUserAsync();
            var randomUser = await environment.GenerateUniqueUserAsync();

            var mutuallyAttendedGathering = await environment.GeneratePastGatheringAsync(user, randomUser);
            var unattendedGathering = await environment.GeneratePastGatheringAsync(randomUser);

            var mutualGatheringSnapshot = await environment.GenerateSnapshotAsync(mutuallyAttendedGathering, randomUser);
            var unattendedGatheringSnapshot = await environment.GenerateSnapshotAsync(unattendedGathering, randomUser);

            // Act
            var gallery = await director.GetGalleryAsync(user.Id, unattendedGathering.Id);

            // Assert
            Assert.Empty(gallery.Snapshots);
        }

        [Fact]
        public async Task GetGalleryAsync_MutualGuest_ReturnsGallery()
        {
            // Arrange
            var user = await environment.GenerateUniqueUserAsync();
            var randomUser = await environment.GenerateUniqueUserAsync();

            var mutuallyAttendedGathering = await environment.GeneratePastGatheringAsync(user, randomUser);
            var unattendedGathering = await environment.GeneratePastGatheringAsync(randomUser);

            var mutualGatheringSnapshot = await environment.GenerateSnapshotAsync(mutuallyAttendedGathering, randomUser);
            var unattendedGatheringSnapshot = await environment.GenerateSnapshotAsync(unattendedGathering, randomUser);

            // Act
            var gallery = await director.GetGalleryAsync(user.Id, mutuallyAttendedGathering.Id);

            // Assert
            Assert.Single(gallery.Snapshots);
        }

        [Fact]
        public async Task GetGalleryAsync_Blocked_Fails()
        {
            // Arrange
            var user = await environment.GenerateUniqueUserAsync();
            var enemy = await environment.GenerateUniqueUserAsync();
            await environment.ForceEnemiesAsync(user, enemy);

            var randomGathering = await environment.GeneratePastGatheringAsync(enemy);

            // Act
            var nest = director.GetGalleryAsync(user.Id, randomGathering.Id);

            // Assert
            await Assert.ThrowsAnyAsync<HollowException>(async () => await nest);
        }

        [Fact]
		public async Task AddSnapshotAsync_InvalidGathering_Fails()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var sneakyUser = await environment.GenerateUniqueUserAsync();
			var gathering = await environment.GenerateUpcomingGatheringAsync(host);

			// Act
			var addSnapshotSync = director.AddSnapshotAsync(sneakyUser.Id, gathering.Id, new(0));

			// Assert
			await Assert.ThrowsAnyAsync<HollowException>(async () => await addSnapshotSync);
		}

		[Fact]
		public async Task RemoveSnapshotAsync_OwnedSnapshot_Succeeds()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var gathering = await environment.GenerateUpcomingGatheringAsync(host);
			var snapshot = await environment.GenerateSnapshotAsync(gathering, host);

			// Act
			await director.DeleteSnapshotAsync(host.Id, snapshot.Id);

			// Assert
			var serverSnapshots = await director.GetGalleryAsync(host.Id, gathering.Id);
			Assert.Empty(serverSnapshots.Snapshots);
		}

		[Fact]
		public async Task RemoveSnapshotAsync_UnownedSnapshot_Fails()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var sneakyUser = await environment.GenerateUniqueUserAsync();
			var gathering = await environment.GenerateUpcomingGatheringAsync(host);
			var snapshot = await environment.GenerateSnapshotAsync(gathering, host);

			// Act
			var removeSnapshotSync = director.DeleteSnapshotAsync(sneakyUser.Id, snapshot.Id);

			// Assert
			await Assert.ThrowsAnyAsync<HollowException>(async () => await removeSnapshotSync);
		}

		[Fact]
		public async Task RateSnapshotAsync_ValidSnapshot_Succeeds()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var guest = await environment.GenerateUniqueUserAsync();
			var gathering = await environment.GenerateOngoingGatheringAsync(host, guest);
			var coolSnapshot = await environment.GenerateSnapshotAsync(gathering, host);
			var uglySnapshot = await environment.GenerateSnapshotAsync(gathering, host);

			// Act
			await director.AcclaimSnapshotAsync(guest.Id, coolSnapshot.Id, SnapshotAcclaim.Acclaim);

			// Assert
			var serverSnapshots = await director.GetGalleryAsync(host.Id, gathering.Id);
			
			var serverCoolSnapshot = serverSnapshots.Snapshots.Find(snapshot => snapshot.Id.Equals(coolSnapshot.Id));
			Assert.Equal(1, serverCoolSnapshot.Acclaim);

			var serverUglySnapshot = serverSnapshots.Snapshots.Find(snapshot => snapshot.Id.Equals(uglySnapshot.Id));
			Assert.Equal(0, serverUglySnapshot.Acclaim);
		}

		[Fact]
		public async Task GetUserColumnAsync_ReturnsColumn()
		{
			// Arrange
			var host = await environment.GenerateUniqueUserAsync();
			var companion = await environment.GenerateUniqueUserAsync();
			await environment.ForceCompanionshipAsync(host, companion);

			var gathering = await environment.GeneratePastGatheringAsync(host);
			var someSnapshot = await environment.GenerateSnapshotAsync(gathering, host);
			var anotherSnapshot = await environment.GenerateSnapshotAsync(gathering, host);

			// Act
			var column = await director.GetWallAsync(companion.Id, 100, 0);

			// Assert
			Assert.Single(column.Headers);
			Assert.Equal(gathering.Id, column.Headers[0].Id);

			Assert.Equal(2, column.Snapshots.Count);
			var serverSomeSnapshot = column.Snapshots.Find(snapshot => snapshot.Id.Equals(someSnapshot.Id));
			Assert.Equal(gathering.Id, serverSomeSnapshot.GatheringId);
			Assert.Equal(someSnapshot.Id, serverSomeSnapshot.Id);
		}

		[Fact]
		public async Task GetUserColumnAsync_ExcludingGathering_ReturnsColumn()
		{
			// Arrange
			var host1 = await environment.GenerateUniqueUserAsync();
			var host2 = await environment.GenerateUniqueUserAsync();
			var companion = await environment.GenerateUniqueUserAsync();
			await environment.ForceCompanionshipAsync(host1, host2, companion);

			var gathering1 = await environment.GenerateOngoingGatheringAsync(host1);
			var seenSnapshot = await environment.GenerateSnapshotAsync(gathering1, host1);

			var gathering2 = await environment.GeneratePastGatheringAsync(host2);
			var unseenSnapshot = await environment.GenerateSnapshotAsync(gathering1, host2);

			// Act
			var column = await director.GetWallAsync(companion.Id, 100, 1);

			// Assert
			Assert.Single(column.Headers);
			Assert.Equal(gathering2.Id, column.Headers[0].Id);

			Assert.Single(column.Snapshots);
			var serverSomeSnapshot = column.Snapshots.Find(snapshot => snapshot.Id.Equals(unseenSnapshot.Id));
			Assert.Equal(gathering2.Id, serverSomeSnapshot.GatheringId);
			Assert.Equal(unseenSnapshot.Id, serverSomeSnapshot.Id);
		}
	}
}
