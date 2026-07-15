using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Interfaces
{
    public interface IOneSignalService
    {
        Task<string> CreateUserAsync(Guid externalId, string email, CancellationToken cancellationToken = default);
        Task DeleteUserAsync(Guid externalId, CancellationToken cancellationToken = default);
        Task AddTagAsync(Guid externalId, string key, string value, CancellationToken cancellationToken = default);
        Task RemoveTagAsync(Guid externalId, string key, CancellationToken cancellationToken = default);
        Task SendTemplatedEmailAsync(string templateId, IEnumerable<string> emailTo, object customData, CancellationToken cancellationToken = default);
        Task TrackEventAsync(Guid externalId, string eventName, CancellationToken cancellationToken = default);
    }
}
