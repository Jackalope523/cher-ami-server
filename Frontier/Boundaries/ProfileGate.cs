using CrazyLizard.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Boundaries
{
    public record CoreBlockedUser(long UserId, string FullName, DateTimeOffset DateBlocked);

    #region Gates

    public interface IProfileRepository
    {
        Task<List<CoreBlockedUser>> GetBlockedUsersAsync(long userId);
        Task<List<User>> GetUsersBlockingAsync(long userId);

        Task BlockUserAsync(long userId, long targetId, DateTimeOffset time);
        Task UnblockUserAsync(long userId, long targetId);

        Task<DateTimeOffset> BlockedSince(long userId, long targetId);
    }

	public interface IProfileService
    {
        Task<List<CoreBlockedUser>> GetBlockedUsersAsync(long userId);

        Task BlockUserAsync(long userId, long targetId);
        Task UnblockUserAsync(long userId, long targetId);
    }

	#endregion
}

