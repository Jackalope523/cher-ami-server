using CherAmiAPI.Entities;
using CherAmiAPI.Exceptions;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Shared.Responses;
using CherAmiAPI.Shared.SharedMappers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Services
{
    public class UserService(IUserRepository userRepository, UserItemMapper userItemMapper)
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
    }
}
