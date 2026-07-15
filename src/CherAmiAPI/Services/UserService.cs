using CherAmiAPI.Entities;
using CherAmiAPI.Exceptions;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Shared.Responses;
using CherAmiAPI.Shared.SharedMappers;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerService = Stripe.CustomerService;

namespace CherAmiAPI.Services
{
    public class UserService(
        IUserRepository userRepository,
        IRecipientRepository recipientRepository,
        IPostRepository postRepository,
        UserItemMapper userItemMapper,
        IImageService imageService,
        IOneSignalService oneSignalService,
        CustomerService customerService)
    {
        public async Task<User> GetUserAsync(long requesterId, long targetId, CancellationToken cancellationToken = default)
        {
            if (requesterId != targetId && !await userRepository.ShareCommonCircleAsync(cancellationToken, requesterId, targetId))
                throw new NoAccessException($"User {requesterId} can not access this user {targetId}.");

            return await userRepository.GetUserWithRecipientsAsync(targetId, cancellationToken);
        }

        public async Task<List<UserItem>> GetBlockedUsersAsync(long requesterId, CancellationToken cancellationToken = default)
        {
            List<User> blockedUsers = await userRepository.GetBlockedUsers(requesterId, cancellationToken);

            return [.. blockedUsers.Select(userItemMapper.FromEntity)];
        }

        public async Task UpdateUserAsync(long userId, string firstName, string lastName, IFormFile avatar = null, CancellationToken cancellationToken = default)
        {
            string avatarPath = null;
            DateTimeOffset? avatarTimestamp = null;

            if (avatar != null)
            {
                using MemoryStream stream = new();
                await avatar.CopyToAsync(stream, cancellationToken);

                avatarPath = $"users/{userId}/avatar.jpg";
                avatarTimestamp = DateTimeOffset.UtcNow;

                await imageService.UploadImageAsync(avatarPath, stream);
            }

            await userRepository.UpdateProfileAsync(userId, firstName, lastName, avatarPath, avatarTimestamp, cancellationToken);
        }

        public async Task UpdateAvatarAsync(long userId, IFormFile image, CancellationToken cancellationToken = default)
        {
            using MemoryStream stream = new();
            await image.CopyToAsync(stream, cancellationToken);

            string path = $"users/{userId}/avatar.jpg";

            await imageService.UploadImageAsync(path, stream);
            await userRepository.SetAvatarAsync(userId, path, DateTimeOffset.UtcNow, cancellationToken);
        }

        public async Task<MemoryStream> GetAvatarAsync(long requesterId, long targetId, CancellationToken cancellationToken = default)
        {
            if (requesterId != targetId && !await userRepository.ShareCommonCircleAsync(cancellationToken, requesterId, targetId))
                throw new NoAccessException($"User {requesterId} can not access this avatar.");

            string path = await userRepository.GetAvatarPathAsync(targetId, cancellationToken);

            return await imageService.DownloadImageAsync(path);
        }

        public async Task BlockUserAsync(long blockerId, long targetId, CancellationToken cancellationToken = default)
        {
            if (blockerId == targetId)
                throw new NoPermissionException($"A user can not block themselves.");

            if (await userRepository.HasBlockedAsync(blockerId, targetId, cancellationToken))
                throw new ConflictException($"A user can not block another user multiple times.");

            await userRepository.CreateBlockAsync(blockerId, targetId, cancellationToken);
        }

        public async Task UnblockUserAsync(long blockerId, long targetId, CancellationToken cancellationToken = default)
        {
            if (blockerId == targetId)
                throw new NoPermissionException($"A user can not unblock themselves.");

            if (!await userRepository.RemoveBlockAsync(blockerId, targetId, cancellationToken))
                throw new NotFoundException($"Could not find a block on that user.");
        }

        public async Task DeleteUserAsync(long userId, CancellationToken cancellationToken = default)
        {
            User user = await userRepository.GetUserAsync(userId, cancellationToken);

            List<string> recipientAvatars = await recipientRepository.GetAvatarPathsByManagerAsync(userId, cancellationToken);
            List<string> postImages = await postRepository.GetImagePathsByAuthorAsync(userId, cancellationToken);

            List<string> imagesToDelete = [.. recipientAvatars, .. postImages];

            if (user.AvatarPath != null)
            {
                imagesToDelete.Add(user.AvatarPath);
            }

            await oneSignalService.DeleteUserAsync(user.ExternalId, cancellationToken);
            await customerService.DeleteAsync(user.StripeCustomerId, cancellationToken: cancellationToken);

            await userRepository.PurgeUserDataAsync(userId, cancellationToken);
            await imageService.DeleteImagesAsync(imagesToDelete);
        }
    }
}
