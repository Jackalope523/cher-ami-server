using CherAmiAPI.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Interfaces
{
    public interface ICircleRepository
    {
        Task<long?> GetCircleIdOfUserAsync(long userId, CancellationToken cancellationToken = default);
        Task<Circle> GetCircleOfUserAsync(long userId, CancellationToken cancellationToken = default);
        Task<Circle> GetCircleWithContributorsAsync(long circleId, List<long> excludedUserIds, CancellationToken cancellationToken = default);
        Task<string> GetCircleCodeOfUserAsync(long userId, CancellationToken cancellationToken = default);
        Task<long> GetCircleIdByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<bool> IsUserInCircleAsync(long userId, long circleId, CancellationToken cancellationToken = default);
        Task<string> GetHeaderPathAsync(long circleId, CancellationToken cancellationToken = default);
        Task AddCircleAsync(Circle circle, CancellationToken cancellationToken = default);
        Task AddIssueAsync(Issue issue, CancellationToken cancellationToken = default);
        Task UpdateCircleAsync(long circleId, string title, string headerPath = null, DateTimeOffset? headerTimestamp = null, CancellationToken cancellationToken = default);
        Task SetHeaderAsync(long circleId, string headerPath, DateTimeOffset headerTimestamp, CancellationToken cancellationToken = default);
        Task SetCircleCodeAsync(long circleId, string code, CancellationToken cancellationToken = default);
        Task AddUserToCircleAsync(long userId, long circleId, CancellationToken cancellationToken = default);
        Task RemoveUserFromCircleAsync(long userId, CancellationToken cancellationToken = default);
    }
}
