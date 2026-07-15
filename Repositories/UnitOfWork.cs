using CherAmiAPI.Contexts;
using CherAmiAPI.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Repositories
{
    public class UnitOfWork(ApplicationDbContext ctx) : IUnitOfWork
    {
        public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await operation();
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
