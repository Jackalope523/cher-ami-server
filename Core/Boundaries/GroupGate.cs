using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Core.Boundaries
{
    #region Schemas

    public enum GroupPlan
    { DigitalOnly, Newspaper_30, Newspaper_60 }

    public enum SegmentFrequency
    { Monthly }

    public record CoreGroup(long Id, long HostId, string InviteCode, string Title,
        DateTimeOffset DateCreated, GroupPlan Plan, SegmentFrequency Frequency,
        bool IsPendingDeletion)
        : CoreOnlyData();

    public record GroupShard(long Id, long HostId, string InviteCode, string Title,
        DateTimeOffset DateCreated, GroupPlan Plan, SegmentFrequency Frequency);

    public record CoreGroupMembership(long UserId, DateTimeOffset DateJoined)
        : CoreOnlyData();

    public record GroupMembershipShard(long UserId);

    #endregion

    #region Gates

    public interface IGroupDatabase
    {
        Task<CoreGroup> GetGroupAsync(long groupId);
        Task<List<CoreGroup>> GetGroupsForUserAsync(long userId);

        Task<CoreGroup> CreateGroupAsync(long ownerId, string title);
        Task UpdateGroupAsync(long groupId, List<(string Property, object Value)> edits);
        Task DeleteGroupAsync(long groupId);

        Task<List<CoreGroupMembership>> GetGroupMembersAsync(long groupId);

        Task<CoreGroupMembership> GetGroupMemberAsync(long userId, long groupId);
        Task UpdateGroupMemberAsync(long userId, long groupId, List<(string Property, object Value)> edits);
        Task AddGroupMemberAsync(long userId, long groupId);
        Task DeleteGroupMemberAsync(long userId, long groupId);

        Task SoftDeleteAsync(long groupId);
        Task HardDeleteAsync(long groupId);
    }

    public interface IGroupOperations
    {
        Task<List<GroupShard>> GetUserGroupsAsync(long userId);
        Task<GroupShard> GetGroupInformationAsync(long userId, long groupId);

        Task<GroupShard> CreateGroupAsync(long userId, string groupTitle,
            MemoryStream heroImage);
        Task EditGroupAsync(long userId, long groupId,
            string groupTitle = "",
            MemoryStream header = null);
        Task DeleteGroupAsync(long userId, long groupId);
        
        Task InviteUserAsync(long ownerId, long inviteeId, long groupId);
        Task RemoveUserAsync(long ownerId, long targetId, long groupId);
    }

    #endregion
}
