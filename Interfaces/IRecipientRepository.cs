using CherAmiAPI.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Interfaces
{
    public interface IRecipientRepository
    {
        Task<Recipient> GetRecipientAsync(long recipientId, CancellationToken cancellationToken = default);
        Task AddRecipientAsync(Recipient recipient, CancellationToken cancellationToken = default);
        Task SaveRecipientAsync(Recipient recipient, CancellationToken cancellationToken = default);
        Task RemoveRecipientAsync(Recipient recipient, CancellationToken cancellationToken = default);
        Task<List<Recipient>> GetActiveRecipientsByManagerAsync(long managerId, CancellationToken cancellationToken = default);
        Task<List<string>> GetAvatarPathsByManagerAsync(long managerId, CancellationToken cancellationToken = default);
        Task<int> CountRecipientsOfManagerAsync(long managerId, CancellationToken cancellationToken = default);
        Task DeleteRecipientsOfManagerAsync(long managerId, CancellationToken cancellationToken = default);
    }
}
