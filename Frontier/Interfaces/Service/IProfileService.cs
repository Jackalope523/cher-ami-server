using CrazyLizard.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrazyLizard.Interfaces.Service
{
	public interface IProfileService
    {
        Task<List<User>> GetBlockedUsersAsync(long userId);

        Task BlockUserAsync(long userId, long targetId);
        Task UnblockUserAsync(long userId, long targetId);
    }
}

