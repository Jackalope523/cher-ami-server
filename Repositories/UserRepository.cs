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
    public class UserRepository(ApplicationDbContext ctx) : IUserRepository
    {
        public async Task<bool> ShareCommonCircleAsync(CancellationToken cancellationToken = default, params long[] userIds)
        {
            int count = await ctx.Users
                .Where(x => userIds.Contains(x.Id))
                .Select(x => x.CircleId)
                .Distinct()
                .CountAsync(cancellationToken: cancellationToken);

            return count == 1;
        }

        public async Task<User> GetUserWithRecipientsAsync(long userId, CancellationToken cancellationToken = default)
        {
            return await ctx.Users
                .Where(x => x.Id == userId)
                .Include(x => x.Recipients)
                .SingleAsync(cancellationToken: cancellationToken);
        }

        public async Task<List<User>> GetBlockedUsers(long userId, CancellationToken cancellationToken = default)
        {
            return await ctx.Blocks
                .Where(x => x.BlockerId == userId)
                .Select(x => x.Blocked)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
