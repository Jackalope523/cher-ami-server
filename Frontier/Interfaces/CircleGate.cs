using CrazyLizard.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Core.Boundaries
{
    public record CoreCircleMembership(long UserId, DateTimeOffset DateJoined);


    public record CoreRecipient(long Id, long ManagerId, string Title, string FirstName, string LastName, Address Address = null);


    public record Address(string Street, string UnitNumber,
        string City, string ProvinceOrState,
        string PostalCode, string Country);

    public interface ICircleRepository
    {
        Task<bool> Exists(long circleId);
        Task<bool> Exists(string circleCode);
        Task<Circle> GetCircleAsync(long circleId);
        Task<Circle> GetCircleByCodeAsync(string circleCode);
        Task<Circle> GetCircleForUserAsync(long userId);

        Task<Circle> CreateCircleAsync(long ownerId, string title, IssueSchedule schedule);
        Task UpdateCircleAsync(long circleId, List<(string Property, object Value)> edits);
        Task<string> RerollCircleCode(long circleId);
        Task DeleteCircleAsync(long circleId);

        Task<List<User>> GetCircleContributorsAsync(long circleId);
        Task<List<CoreRecipient>> GetRecipientsForCircleAsync(long circleId);

        Task<bool> HasCircle(long userId);
        Task<bool> IsMemberAsync(long userId, long circleId);
        Task UpdateCircleMemberAsync(long userId, long circleId, List<(string Property, object Value)> edits);
        Task AddCircleMemberAsync(long userId, string circleCode);
        Task RemoveCircleMembershipAsync(long userId);
    }

    public interface ICircleService
    {
        Task<Circle> GetCircleForUserAsync(long userId);
        Task<Circle> GetCircleInformationAsync(long userId, long circleId);

        Task<Circle> CreateCircleAsync(long userId,
            string title,
            IssueSchedule schedule,
            MemoryStream header);
        Task EditCircleAsync(long userId, long circleId,
            string title = "",
           IssueSchedule? schedule = null,
            MemoryStream header = null);
        Task<string> RerollCircleCodeAsync(long userId, long circleId);
        Task DeleteCircleAsync(long userId, long circleId);
        
        Task<List<User>> GetCircleMembers(long userId);
        Task AddMemberAsync(long userId, string circleCode);
        Task RemoveMemberAsync(long userId, long circleId);

        Task<List<CoreRecipient>> GetRecipientsForCircleAsync(long userId);
    }
}
