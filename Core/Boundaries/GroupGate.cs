using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace Core.Boundaries
{
    #region Schemas

    public enum GroupPlan
    { None, Digital, Newspaper_30, Newspaper_60, Magazine_30 }

    public enum IssueSchedule
    { Monthly }

    public record CoreGroup(long Id, string InviteCode, string Title,
        DateTimeOffset DateCreated, GroupPlan Plan, IssueSchedule Schedule,
        bool IsPendingDeletion)
        : CoreOnlyData();

    public record GroupShard(long Id, string InviteCode, string Title,
        DateTimeOffset DateCreated, GroupPlan Plan, IssueSchedule Schedule);


    public enum GroupMembershipType
    { Regular, Owner }

    public record CoreGroupMembership(long UserId, DateTimeOffset DateJoined, GroupMembershipType Type)
        : CoreOnlyData();

    public record GroupMembershipShard(long UserId, DateTimeOffset DateJoined, GroupMembershipType Type);


    public record CoreRecipient(long Id, long ManagerId, bool IsMyself, string FullName = null, DateTimeOffset? DateOfBirth = null, Address Address = null)
        : CoreOnlyData();

    public record RecipientShard(long Id, long ManagerId, bool IsMyself, string FullName = null, DateTimeOffset? DateOfBirth = null, Address Address = null);


    public record Address(string Street, string ApartmentOrSuite,
        string City, string ProvinceOrState,
        string PostalCode, string Country);

    #endregion

    #region Gates

    public interface IGroupDatabase
    {
        Task<CoreGroup> GetGroupAsync(long groupId);
        Task<CoreGroup> GetGroupByCodeAsync(string groupCode);
        Task<List<CoreGroup>> GetGroupsForUserAsync(long userId);

        Task<CoreGroup> CreateGroupAsync(long ownerId, string title, GroupPlan plan, IssueSchedule schedule);
        Task UpdateGroupAsync(long groupId, List<(string Property, object Value)> edits);
        Task<string> RerollGroupCode(long groupId);
        Task DeleteGroupAsync(long groupId);

        Task<List<CoreGroupMembership>> GetGroupMembersAsync(long groupId);
        Task<List<RecipientShard>> GetRecipientsForGroupAsync(long groupId);

        Task<CoreGroupMembership> GetGroupMembershipAsync(long userId, long groupId);
        Task UpdateGroupMemberAsync(long userId, long groupId, List<(string Property, object Value)> edits);
        Task AddGroupMemberAsync(long userId, long groupId);
        Task DeleteGroupMemberAsync(long userId, long groupId);

        Task AddRecipientAsync(long groupId, long userId);
        Task UpdateRecipientAsync(long recipientId, List<(string Property, object Value)> edits);
        Task DeleteRecipientAsync(long recipientId);

        Task SoftDeleteAsync(long groupId);
        Task HardDeleteAsync(long groupId);
    }

    public interface IGroupOperations
    {
        Task<List<GroupShard>> GetUserGroupsAsync(long userId);
        Task<GroupShard> GetGroupInformationAsync(long userId, long groupId);

        Task<GroupShard> CreateGroupAsync(long userId, string groupTitle,
            GroupPlan plan, IssueSchedule schedule,
            MemoryStream heroImage);
        Task EditGroupAsync(long userId, long groupId,
            string groupTitle = "",
            GroupPlan? plan = null, IssueSchedule? schedule = null,
            MemoryStream header = null);
        Task<string> RerollGroupCodeAsync(long userId, long groupId);
        Task DeleteGroupAsync(long userId, long groupId);
        
        Task<List<GroupMembershipShard>> GetMembersForGroupAsync(long userId, long groupId);
        Task SendInvitationAsync(long userId, string phoneNumber = null, string email = null);
        Task JoinGroupAsync(long userId, string groupCode);
        Task RemoveMemberAsync(long userId, long targetId, long groupId);

        Task<List<RecipientShard>> GetRecipientsForGroupAsync(long userId, long groupId);
        Task AddRecipientAsync(long userId, long groupId);
        Task EditRecipientAsync(long recipientId, List<(string Property, object Value)> edits);
        Task RemoveRecipientAsync(long recipientId);
    }

    #endregion
}
