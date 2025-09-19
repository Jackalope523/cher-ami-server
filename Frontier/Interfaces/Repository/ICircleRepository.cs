using CrazyLizard.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrazyLizard.Interfaces.Repository
{
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
        Task<List<Recipient>> GetRecipientsForCircleAsync(long circleId);

        Task<bool> HasCircle(long userId);
        Task<bool> IsMemberAsync(long userId, long circleId);
        Task UpdateCircleMemberAsync(long userId, long circleId, List<(string Property, object Value)> edits);
        Task AddCircleMemberAsync(long userId, string circleCode);
        Task RemoveCircleMembershipAsync(long userId);
    }
}
