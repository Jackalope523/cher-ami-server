using CherAmiAPI.Entities;
using CherAmiAPI.Exceptions;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Services;
using CherAmiAPI.Shared.Responses;
using CherAmiAPI.Shared.SharedMappers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Stripe;
using System.Text;

namespace CherAmiAPI.UnitTests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepository = new(MockBehavior.Strict);
        private readonly Mock<IRecipientRepository> _recipientRepository = new(MockBehavior.Strict);
        private readonly Mock<IPostRepository> _postRepository = new(MockBehavior.Strict);
        private readonly Mock<IImageService> _imageService = new(MockBehavior.Strict);
        private readonly Mock<IOneSignalService> _oneSigalService = new(MockBehavior.Strict);
        private readonly Mock<CustomerService> _customerService = new(MockBehavior.Strict);

        private readonly IConfiguration _config;
        private readonly UserItemMapper _userItemMapper;

        private UserService CreateSut() => new(_userRepository.Object, _recipientRepository.Object, _postRepository.Object, _userItemMapper, _imageService.Object, _oneSigalService.Object, _customerService.Object);

        public UserServiceTests()
        {
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["APP_SERVICE_URI"] = "https://api.test"
                })
                .Build();

            _userItemMapper = new UserItemMapper(_config);
        }

        [Fact]
        public async Task GetUserAsync_WhenRequesterIsTarget_ReturnsFetchedUser()
        {
            const long userId = 5;
            User expected = new() { Id = userId };

            _userRepository
                .Setup(r => r.GetUserWithRecipientsAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            User actual = await CreateSut().GetUserAsync(userId, userId, TestContext.Current.CancellationToken);

            Assert.Same(expected, actual);
        }

        [Fact]
        public async Task GetUserAsync_WhenRequesterIsTarget_SkipsAccessCheck()
        {
            const long userId = 5;
            _userRepository
                .Setup(r => r.GetUserWithRecipientsAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { Id = userId });

            await CreateSut().GetUserAsync(userId, userId, TestContext.Current.CancellationToken);

            _userRepository.Verify(
                r => r.ShareCommonCircleAsync(It.IsAny<CancellationToken>(), It.IsAny<long[]>()),
                Times.Never);
        }

        [Fact]
        public async Task GetUserAsync_WhenDifferentUsersShareCircle_ReturnsFetchedUser()
        {
            const long requesterId = 1;
            const long targetId = 2;
            User expected = new() { Id = targetId };
            _userRepository
                .Setup(r => r.ShareCommonCircleAsync(It.IsAny<CancellationToken>(), It.IsAny<long[]>()))
                .ReturnsAsync(true);
            _userRepository
                .Setup(r => r.GetUserWithRecipientsAsync(targetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            User actual = await CreateSut().GetUserAsync(requesterId, targetId, TestContext.Current.CancellationToken);

            Assert.Same(expected, actual);
        }

        [Fact]
        public async Task GetUserAsync_WhenDifferentUsersShareCircle_ChecksBothUsersShareACircle()
        {
            const long requesterId = 1;
            const long targetId = 2;
            _userRepository
                .Setup(r => r.ShareCommonCircleAsync(It.IsAny<CancellationToken>(), It.IsAny<long[]>()))
                .ReturnsAsync(true);
            _userRepository
                .Setup(r => r.GetUserWithRecipientsAsync(targetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { Id = targetId });

            await CreateSut().GetUserAsync(requesterId, targetId, TestContext.Current.CancellationToken);

            _userRepository.Verify(
                r => r.ShareCommonCircleAsync(
                    It.IsAny<CancellationToken>(),
                    It.Is<long[]>(ids => ids.Length == 2 && ids[0] == requesterId && ids[1] == targetId)),
                Times.Once);
        }

        [Fact]
        public async Task GetUserAsync_WhenDifferentUsersDoNotShareCircle_ThrowsNoAccessException()
        {
            const long requesterId = 1;
            const long targetId = 2;
            _userRepository
                .Setup(r => r.ShareCommonCircleAsync(It.IsAny<CancellationToken>(), It.IsAny<long[]>()))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<NoAccessException>(
                () => CreateSut().GetUserAsync(requesterId, targetId, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task GetUserAsync_WhenAccessDenied_DoesNotFetchUser()
        {
            const long requesterId = 1;
            const long targetId = 2;
            _userRepository
                .Setup(r => r.ShareCommonCircleAsync(It.IsAny<CancellationToken>(), It.IsAny<long[]>()))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<NoAccessException>(
                () => CreateSut().GetUserAsync(requesterId, targetId, TestContext.Current.CancellationToken));

            _userRepository.Verify(
                r => r.GetUserWithRecipientsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetUserAsync_PassesCancellationTokenToRepository()
        {
            const long userId = 5;
            using CancellationTokenSource cts = new();
            _userRepository
                .Setup(r => r.GetUserWithRecipientsAsync(userId, cts.Token))
                .ReturnsAsync(new User { Id = userId });

            await CreateSut().GetUserAsync(userId, userId, cts.Token);

            _userRepository.Verify(
                r => r.GetUserWithRecipientsAsync(userId, cts.Token),
                Times.Once);
        }

        [Fact]
        public async Task GetBlockedUsersAsync_WhenRequesterHasNoBlockedUsers_ReturnsEmptyList()
        {
            const long userId = 5;
            _userRepository
                .Setup(r => r.GetBlockedUsers(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User>());

            List<UserItem> result = await CreateSut().GetBlockedUsersAsync(userId, TestContext.Current.CancellationToken);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBlockedUsersAsync_MapsBlockedUsersWithAvatarUrl()
        {
            const long userId = 5;
            List<User> blocked =
            [
                new User { Id = 10, FirstName = "Blocked", AvatarPath = "users/10/avatar/avatar.jpg" },
                new User { Id = 11, FirstName = "NoAvatar", AvatarPath = null },
            ];
            _userRepository
                .Setup(r => r.GetBlockedUsers(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(blocked);

            List<UserItem> result = await CreateSut().GetBlockedUsersAsync(userId, TestContext.Current.CancellationToken);

            Assert.Equal(2, result.Count);

            // AvatarUrl is the live contract: absolute URL built from APP_SERVICE_URI for users with an avatar, null otherwise.
            Assert.StartsWith("https://api.test/users/10/avatar", result[0].AvatarUrl);
            Assert.Null(result[1].AvatarUrl);

            // AvatarPath is deprecated but still emitted for older app versions.
            Assert.Equal("/users/10/avatar", result[0].AvatarPath);
            Assert.Null(result[1].AvatarPath);
        }

        [Fact]
        public async Task UpdateUserAsync_WithoutAvatar()
        {
            const long userId = 5;
            const string firstName = "Luigi";
            const string lastName = "Mansion";

            _userRepository
                .Setup(r => r.UpdateProfileAsync(
                    userId,
                    firstName,
                    lastName,
                    null,
                    null,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().UpdateUserAsync(
                userId,
                firstName,
                lastName,
                null,
                CancellationToken.None);

            _userRepository.Verify(r => r.UpdateProfileAsync(
                    userId,
                    firstName,
                    lastName,
                    null,
                    null,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _imageService.Verify(x => x.UploadImageAsync(
                    It.IsAny<string>(),
                    It.IsAny<MemoryStream>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateUserAsync_WithAvatar()
        {
            const long userId = 5;
            const string firstName = "Luigi";
            const string lastName = "Mansion";

            MemoryStream stream = new(Encoding.UTF8.GetBytes("fake image content"));

            FormFile avatar = new(
                stream,
                0,
                stream.Length,
                "avatar",
                "avatar.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            _userRepository
                .Setup(r => r.UpdateProfileAsync(
                    userId,
                    firstName,
                    lastName,
                     $"users/{userId}/avatar.jpg",
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _imageService
                .Setup(x => x.UploadImageAsync(
                      $"users/{userId}/avatar.jpg",
                    It.IsAny<MemoryStream>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            await sut.UpdateUserAsync(
                userId,
                firstName,
                lastName,
                avatar,
                CancellationToken.None);

            _imageService.Verify(x => x.UploadImageAsync(
                    $"users/{userId}/avatar.jpg",
                    It.IsAny<MemoryStream>()),
                Times.Once);

            _userRepository.Verify(r => r.UpdateProfileAsync(
                    userId,
                    firstName,
                    lastName,
                    $"users/{userId}/avatar.jpg",
                    It.Is<DateTimeOffset?>(x => x.HasValue),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
