using CherAmiAPI.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> ShareCommonCircleAsync(CancellationToken cancellationToken = default, params long[] userIds);
        Task<User> GetUserAsync(long userId, CancellationToken cancellationToken = default);
        Task<User> GetUserWithRecipientsAsync(long userId, CancellationToken cancellationToken = default);
        Task<User> FindUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<List<User>> GetBlockedUsers(long userId, CancellationToken cancellationToken = default);
        Task<List<long>> GetBlacklistedUserIdsAsync(long userId, CancellationToken cancellationToken = default);
        Task UpdateProfileAsync(long userId, string firstName, string lastName, string avatarPath = null, DateTimeOffset? avatarTimestamp = null, CancellationToken cancellationToken = default);
        Task SetAvatarAsync(long userId, string avatarPath, DateTimeOffset avatarTimestamp, CancellationToken cancellationToken = default);
        Task<string> GetAvatarPathAsync(long userId, CancellationToken cancellationToken = default);
        Task SaveUserAsync(User user, CancellationToken cancellationToken = default);
        Task<bool> HasBlockedAsync(long blockerId, long blockedId, CancellationToken cancellationToken = default);
        Task CreateBlockAsync(long blockerId, long blockedId, CancellationToken cancellationToken = default);
        Task<bool> RemoveBlockAsync(long blockerId, long blockedId, CancellationToken cancellationToken = default);
        Task PurgeUserDataAsync(long userId, CancellationToken cancellationToken = default);
        Task<bool> AnyUsersAsync(CancellationToken cancellationToken = default);
    }
}
