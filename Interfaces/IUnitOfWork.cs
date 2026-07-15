using System;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Interfaces
{
    public interface IUnitOfWork
    {
        Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);
    }
}
