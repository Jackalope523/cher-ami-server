using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Core.Boundaries
{
    #region Schema

    public record CoreOnlyData();

    #endregion

    #region Gates

    public interface IAdminDatabase
	{
        Task<List<CoreGathering>> GetAllActiveGatheringsAsync(DateTimeOffset currentTime);
        Task VoidUserAsync(long userId);
        Task VoidGatheringAsync(long gatheringId);
    }

    #endregion
}

