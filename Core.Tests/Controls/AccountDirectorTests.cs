using Core.Boundaries;
using Core.Controls;
using Core.Entities;

using System;
using System.Threading.Tasks;
using Xunit;

namespace Core.Tests.Controls
{
    public class AccountDirectorTests : CoreTest
    {
		private AccountDirector director;

        public AccountDirectorTests()
        {
			director = environment.Terminal.AccountDirector;
        }

		[Fact]
		public async Task GetUserAsync_ValidId_ReturnsUser()
		{
			// Arrange
			var randomUser = await environment.GenerateUniqueUserAsync();

			// Act
			User user = new(await director.GetCoreUserAsync(randomUser.Id));

			// Assert
			Assert.True(randomUser.Equals(user));
		}

		[Fact]
		public async Task GetUserAsync_InvalidId_ThrowsException()
		{
			// Act
			var userSync = director.GetCoreUserAsync(long.MaxValue);

			// Assert
			await Assert.ThrowsAnyAsync<HollowException>(async () => await userSync);
		}

		[Fact]
		public async Task GetUserAsync_ValidPhoneNumber_ReturnsUser()
		{
			// Arrange
			var randomUser = await environment.GenerateUniqueUserAsync();

			// Act
			User user = new(await director.GetCoreUserAsync(randomUser.PhoneNumber));

			// Assert
			Assert.True(randomUser.Equals(user));
		}

		[Fact]
		public async Task GetUserAsync_InvalidPhoneNumber_ThrowsException()
		{
			// Act
			var userSync = director.GetCoreUserAsync("000");

			// Assert
			await Assert.ThrowsAnyAsync<HollowException>(async () => await userSync);
		}

		[Fact]
		public async Task CreateUserAsync_ValidUser_Succeeds()
		{
			// Arrange
			var newUser = environment.CreateTestUser();

			// Act
			await director.CreateUserAsync(newUser.PhoneNumber,
				newUser.Email, newUser.Name, newUser.DateOfBirth);
			// If no exception is thrown, the test is successful
		}

		[Fact]
		public async Task CreateUserAsync_ExistingUser_ThrowsException()
		{
			// Arrange
			var randomUser = await environment.GenerateUniqueUserAsync();

			// Act
			var userSync = director.CreateUserAsync(randomUser.PhoneNumber,
				randomUser.Email, randomUser.Name, randomUser.DateOfBirth);

			// Assert
			await Assert.ThrowsAnyAsync<HollowException>(async () => await userSync);
		}

		[Fact]
		public async Task EditUserAsync_ValidInput_UpdatesUser()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			string newName = "Henry Old Boy";

			// Act
			await director.EditUserAsync(user.Id, name: newName);

			// Assert
			var updatedUser = await director.GetCoreUserAsync(user.Id);
			Assert.Equal(newName, updatedUser.Name);
		}

		[Fact]
		public async Task DeleteUserAsync_ValidUser_DeletesUser()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();

			// Act
			await director.DeleteUserAsync(user.Id);

			// Assert
			await Assert.ThrowsAnyAsync<HollowException>(async () => await director.GetCoreUserAsync(user.Id));
		}

		[Fact]
		public async Task UpdateUserLocationAsync_UpdatesLocation()
		{
			// Arrange
			var user = await environment.GenerateUniqueUserAsync();
			await user.LastKnownLocation.Sync();
			GeoLocation newLocation = new() { Latitude = -1, Longitude = -1 };

			// Act
			await director.UpdateUserLocationAsync(user.Id, newLocation.Latitude, newLocation.Longitude);
			User updatedUser = new(await director.GetCoreUserAsync(user.Id));

			// Assert
			Assert.NotEqual(await user.LastKnownLocation,
				await updatedUser.LastKnownLocation);
		}
	}
}
