using CrazyLizard.Entities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CrazyLizard.Interfaces.Service
{
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

        Task<List<Recipient>> GetRecipientsForCircleAsync(long userId);
    }
}
