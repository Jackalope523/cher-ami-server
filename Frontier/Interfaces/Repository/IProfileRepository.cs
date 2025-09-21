using CrazyLizard.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrazyLizard.Interfaces.Repository
{
    public interface IProfileRepository
    {
        Task<List<User>> GetBlockedUsersAsync(long userId);
        Task<List<User>> GetUsersBlockingAsync(long userId);

        Task BlockUserAsync(long userId, long targetId, DateTimeOffset time);
        Task UnblockUserAsync(long userId, long targetId);

        Task<DateTimeOffset> BlockedSince(long userId, long targetId);
    }
}

