using System;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Interfaces
{
    public interface IAuthRepository
    {
        Task CreateEmailLoginAsync(string email, string code, DateTimeOffset expiresAt, CancellationToken cancellationToken = default);
        Task<bool> IsEmailLoginCodeValidAsync(string email, string code, CancellationToken cancellationToken = default);
    }
}
