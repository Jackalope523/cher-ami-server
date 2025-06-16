using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Boundaries
{
    #region Schemas

    public record NestShard(List<TwigShard> Twigs,
        long RelativeGatheringId = default);
    public record TwigShard(long GatheringId, DateTimeOffset StartTime);

    public record AgendaShard(List<CardShard> Cards);
    public record CardShard(long GatheringId, DateTimeOffset StartTime, GatheringBond Bond);

    public record BlockedUserShard(long Id, string NameWhenBlocked, DateTimeOffset DateBlocked) :
        UserShard(Id, NameWhenBlocked);

    public record CompanionshipRequestShard(UserShard User, DateTimeOffset Time);

	#endregion

	#region Gates

	public interface INestDatabase
    {
        Task<List<CoreUser>> GetCompanionsAsync(long userId);
        Task<List<CompanionshipRequestShard>> GetIncomingRequestsAsync(long userId);
        Task<List<CompanionshipRequestShard>> GetOutgoingRequestsAsync(long userId);
        Task<List<CoreUser>> GetFollowedUsersAsync(long userId);
        Task<List<CoreUser>> GetUserFollowersAsync(long userId);
        Task<List<BlockedUserShard>> GetBlockedUsersAsync(long userId);
        Task<List<CoreUser>> GetUsersBlockingAsync(long userId);
        Task<List<CoreUser>> GetRecentlyMetAsync(long userId);

        Task FollowUserAsync(long userId, long targetId, DateTimeOffset time);
		Task UnfollowUserAsync(long userId, long targetId);
		Task BlockUserAsync(long userId, long targetId, DateTimeOffset time);
		Task UnblockUserAsync(long userId, long targetId);

        Task<bool> HaveMutualGathering(long userId, long targetId);
        Task<CoreGathering> GetFirstMutualGathering(long userId, long targetId);
        Task<CoreGathering> GetLatestMutualGathering(long userId, long targetId);
        Task<DateTimeOffset> BlockedSince(long userId, long targetId);

        Task<List<long>> ReturnStrangerDangerAsync(long userId, params long[] users);
    }

	public interface INestOperations
    {
        Task<NestShard> GetNestAsync(long userId, long targetId);

        Task<AgendaShard> GetUserAgendaAsync(long userId);
        Task<IDictionary<long, AgendaShard>> GetCompanionAgendasAsync(long userId);

        Task<List<UserShard>> GetCompanionsAsync(long userId);
        Task<List<CompanionshipRequestShard>> GetIncomingCompanionshipRequestsAsync(long userId);
        Task<List<CompanionshipRequestShard>> GetOutgoingCompanionshipRequestsAsync(long userId);
        Task<List<UserShard>> GetRecentlyMetAsync(long userId);
        Task<List<BlockedUserShard>> GetBlockedUsersAsync(long userId);

        Task AcceptOrRequestCompanionshipAsync(long userId, long targetId);
        Task RequestCompanionshipAsync(long userId, string code);
        Task DenyOrRemoveUserAsync(long userId, long targetId);
        Task BlockUserAsync(long userId, long targetId);
        Task UnblockUserAsync(long userId, long targetId);

        Task<bool> AuthorisedToFollow(long userId, long targetId);
    }

	#endregion
}

