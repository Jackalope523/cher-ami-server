using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Repositories
{
    public class RecipientRepository(ApplicationDbContext ctx) : IRecipientRepository
    {
        public async Task<Recipient> GetRecipientAsync(long recipientId, CancellationToken cancellationToken = default)
        {
            return await ctx.Recipients
                .Where(x => x.Id == recipientId)
                .SingleAsync(cancellationToken: cancellationToken);
        }

        public async Task AddRecipientAsync(Recipient recipient, CancellationToken cancellationToken = default)
        {
            ctx.Recipients.Add(recipient);
            await ctx.SaveChangesAsync(cancellationToken);
        }

        public async Task SaveRecipientAsync(Recipient recipient, CancellationToken cancellationToken = default)
        {
            await ctx.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveRecipientAsync(Recipient recipient, CancellationToken cancellationToken = default)
        {
            ctx.Recipients.Remove(recipient);
            await ctx.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Recipient>> GetActiveRecipientsByManagerAsync(long managerId, CancellationToken cancellationToken = default)
        {
            return await ctx.Recipients
                .Where(x => x.ManagerId == managerId && !x.SoftDeleted)
                .ToListAsync(cancellationToken: cancellationToken);
        }

        public async Task<List<string>> GetAvatarPathsByManagerAsync(long managerId, CancellationToken cancellationToken = default)
        {
            return await ctx.Recipients
                .Where(x => x.ManagerId == managerId)
                .Select(x => x.AvatarPath)
                .ToListAsync(cancellationToken: cancellationToken);
        }

        public async Task<int> CountRecipientsOfManagerAsync(long managerId, CancellationToken cancellationToken = default)
        {
            return await ctx.Recipients
                .Where(x => x.ManagerId == managerId)
                .CountAsync(cancellationToken: cancellationToken);
        }

        public async Task DeleteRecipientsOfManagerAsync(long managerId, CancellationToken cancellationToken = default)
        {
            await ctx.Recipients
                .Where(x => x.ManagerId == managerId)
                .ExecuteDeleteAsync(cancellationToken: cancellationToken);
        }
    }
}
