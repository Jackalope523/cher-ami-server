using CherAmiAPI.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> ShareCommonCircleAsync(CancellationToken cancellationToken = default, params long[] userIds);
        Task<User> GetUserWithRecipientsAsync(long userId, CancellationToken cancellationToken = default);
        Task<List<User>> GetBlockedUsers(long userId, CancellationToken cancellationToken = default);
    }
}
