using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Boundaries
{
    #region Schemas

    public record ProfileShard(List<ProfileSegmentShard> Segments);
    public record ProfileSegmentShard(long SegmentId, DateTimeOffset StartDate, DateTimeOffset EndDate);

	#endregion

	#region Gates

	public interface IProfileDatabase
    {

    }

	public interface IProfileOperations
    {
        Task<ProfileShard> GetProfileAsync(long userId, long targetId);
    }

	#endregion
}

