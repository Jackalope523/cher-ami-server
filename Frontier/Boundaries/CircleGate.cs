using LazyLizardBackend.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace Core.Boundaries
{
    #region Schemas

    public enum IssueSchedule
    { Monthly }

    public record CoreCircle(long Id, string InviteCode, string Title,
        DateTimeOffset DateCreated, CirclePlan Plan, IssueSchedule Schedule,
        bool IsPendingDeletion)
        : CoreOnlyData();

    public record CoreCircleMembership(long UserId, DateTimeOffset DateJoined, CircleMembershipType Type)
        : CoreOnlyData();


    public record CoreRecipient(long Id, long ManagerId, string Title, string FirstName, string LastName, DateTimeOffset DateOfBirth, Address Address = null)
        : CoreOnlyData();


    public record Address(string Street, string ApartmentOrSuite,
        string City, string ProvinceOrState,
        string PostalCode, string Country);

    #endregion

    #region Gates

    public interface ICircleRepository
    {
        Task<CoreCircle> GetCircleAsync(long circleId);
        Task<CoreCircle> GetCircleByCodeAsync(string circleCode);
        Task<List<CoreCircle>> GetCirclesForUserAsync(long userId);

        Task<CoreCircle> CreateCircleAsync(long ownerId, string title, CirclePlan plan, IssueSchedule schedule);
        Task UpdateCircleAsync(long circleId, List<(string Property, object Value)> edits);
        Task<string> RerollCircleCode(long circleId);
        Task DeleteCircleAsync(long circleId);

        Task<List<CoreCircleMembership>> GetCircleMembersAsync(long circleId);
        Task<List<CoreRecipient>> GetRecipientsForCircleAsync(long circleId);

        Task<CoreCircleMembership> GetCircleMembershipAsync(long userId, long circleId);
        Task UpdateCircleMemberAsync(long userId, long circleId, List<(string Property, object Value)> edits);
        Task AddCircleMemberAsync(long userId, string circleCode);
        Task RemoveCircleMembershipAsync(long userId, long circleId);

        Task AddRecipientAsync(long circleId, long recipientId);
        Task RemoveRecipientAsync(long circleId, long recipientId);
        Task UpdateRecipientAsync(long recipientId, List<(string Property, object Value)> edits);
        Task CreateRecipient(CoreRecipient recipient);
        Task DeleteRecipientAsync(long recipientId);
    }

    public interface ICircleService
    {
        Task<List<CoreCircle>> GetUserCirclesAsync(long userId);
        Task<CoreCircle> GetCircleInformationAsync(long userId, long circleId);

        Task<CoreCircle> CreateCircleAsync(long userId,
            string title,
            CirclePlan plan, IssueSchedule schedule,
            MemoryStream header);
        Task EditCircleAsync(long userId, long circleId,
            string title = "",
            CirclePlan? plan = null, IssueSchedule? schedule = null,
            MemoryStream header = null);
        Task<string> RerollCircleCodeAsync(long userId, long circleId);
        Task DeleteCircleAsync(long userId, long circleId);
        
        Task<List<CoreCircleMembership>> GetCircleMembers(long userId, long circleId);
        Task JoinCircleAsync(long userId, string circleCode);
        Task RemoveMemberAsync(long userId, long circleId);

        Task<List<CoreRecipient>> GetRecipientsForCircleAsync(long userId, long circleId);
        Task AddRecipientAsync(long circleId, long recipientId);
        Task RemoveRecipientAsync(long circleId, long recipientId);
        Task CreateRecipient(CoreRecipient recipient);
        Task DeleteRecipientAsync(long recipientId);
        Task EditRecipientAsync(long recipientId, List<(string Property, object Value)> edits);

    }

    #endregion
}
