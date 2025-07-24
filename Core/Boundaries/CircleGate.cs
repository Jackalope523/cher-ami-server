using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace Core.Boundaries
{
    #region Schemas

    public enum CirclePlan
    { None, Digital, Newspaper_30, Newspaper_60, Magazine_30 }

    public enum IssueSchedule
    { Monthly }

    public record CoreCircle(long Id, string InviteCode, string Title,
        DateTimeOffset DateCreated, CirclePlan Plan, IssueSchedule Schedule,
        bool IsPendingDeletion)
        : CoreOnlyData();

    public record CircleShard(long Id, string InviteCode, string Title,
        DateTimeOffset DateCreated, CirclePlan Plan, IssueSchedule Schedule);


    public enum CircleMembershipType
    { Regular, Owner }

    public record CoreCircleMembership(long UserId, DateTimeOffset DateJoined, CircleMembershipType Type)
        : CoreOnlyData();

    public record CircleMembershipShard(long UserId, DateTimeOffset DateJoined, CircleMembershipType Type);


    public record CoreRecipient(long Id, long ManagerId, string Title, string FirstName, string LastName, DateTimeOffset? DateOfBirth = null, Address Address = null)
        : CoreOnlyData();

    public record RecipientShard(long Id, long ManagerId, string FullName = null, DateTimeOffset? DateOfBirth = null, Address Address = null);


    public record Address(string Street, string ApartmentOrSuite,
        string City, string ProvinceOrState,
        string PostalCode, string Country);

    #endregion

    #region Gates

    public interface ICircleDatabase
    {
        Task<CoreCircle> GetCircleAsync(long circleId);
        Task<CoreCircle> GetCircleByCodeAsync(string circleCode);
        Task<List<CoreCircle>> GetCirclesForUserAsync(long userId);

        Task<CoreCircle> CreateCircleAsync(long ownerId, string title, CirclePlan plan, IssueSchedule schedule);
        Task UpdateCircleAsync(long circleId, List<(string Property, object Value)> edits);
        Task<string> RerollCircleCode(long circleId);
        Task DeleteCircleAsync(long circleId);

        Task<List<CoreCircleMembership>> GetCircleMembersAsync(long circleId);
        Task<List<RecipientShard>> GetRecipientsForCircleAsync(long circleId);

        Task<CoreCircleMembership> GetCircleMembershipAsync(long userId, long circleId);
        Task UpdateCircleMemberAsync(long userId, long circleId, List<(string Property, object Value)> edits);
        Task AddCircleMemberAsync(long userId, long circleId);
        Task RemoveCircleMembershipAsync(long userId, long circleId);

        Task AddRecipientAsync(long circleId, string title, string firstName, string lastName, string streetAddress, string city, string provinceOrState, string postalCode, string country);
        Task UpdateRecipientAsync(long recipientId, List<(string Property, object Value)> edits);
        Task DeleteRecipientAsync(long recipientId);
    }

    public interface ICircleOperations
    {
        Task<List<CircleShard>> GetUserCirclesAsync(long userId);
        Task<CircleShard> GetCircleInformationAsync(long userId, long circleId);

        Task<CircleShard> CreateCircleAsync(long userId,
            string title,
            CirclePlan plan, IssueSchedule schedule,
            MemoryStream header);
        Task EditCircleAsync(long userId, long circleId,
            string title = "",
            CirclePlan? plan = null, IssueSchedule? schedule = null,
            MemoryStream header = null);
        Task<string> RerollCircleCodeAsync(long userId, long circleId);
        Task DeleteCircleAsync(long userId, long circleId);
        
        Task<List<CircleMembershipShard>> GetMembersForCircleAsync(long userId, long circleId);
        Task SendInvitationAsync(long userId, string phoneNumber = null, string email = null);
        Task JoinCircleAsync(long userId, string circleCode);
        Task RemoveMemberAsync(long userId, long targetId, long circleId);

        Task<List<RecipientShard>> GetRecipientsForCircleAsync(long userId, long circleId);
        Task AddRecipientAsync(long userId, long circleId);
        Task EditRecipientAsync(long recipientId, List<(string Property, object Value)> edits);
        Task RemoveRecipientAsync(long recipientId);
    }

    #endregion
}
