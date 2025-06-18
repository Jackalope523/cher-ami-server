using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Boundaries
{
    #region Gates

	public interface IDebugDatabase
	{
        Task DrainDatabaseAsync();
        Task VoidUserAsync(long userId);
    }

	public interface IDebugOperations
	{
        Task SeedDatabaseAsync();
    }

    #endregion
}

