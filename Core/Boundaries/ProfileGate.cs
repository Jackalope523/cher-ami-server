using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Boundaries
{
    #region Schemas

    public record ProfileShard(List<ProfileIssueShard> Issues);

    public record ProfileIssueShard(long IssueId, DateTimeOffset EndDate);

    public record BlockedUserShard(long Id, string Name, DateTimeOffset DateBlocked);

    #endregion

    #region Gates

    public interface IProfileDatabase
    {
        Task<List<BlockedUserShard>> GetBlockedUsersAsync(long userId);
        Task<List<CoreUser>> GetUsersBlockingAsync(long userId);

        Task BlockUserAsync(long userId, long targetId, DateTimeOffset time);
        Task UnblockUserAsync(long userId, long targetId);

        Task<DateTimeOffset> BlockedSince(long userId, long targetId);
    }

	public interface IProfileOperations
    {
        Task<ProfileShard> GetProfileAsync(long userId, long targetId);

        Task<List<BlockedUserShard>> GetBlockedUsersAsync(long userId);

        Task BlockUserAsync(long userId, long targetId);
        Task UnblockUserAsync(long userId, long targetId);
    }

	#endregion
}

