using CrazyLizard.Contexts;
using Microsoft.EntityFrameworkCore;
using CrazyLizard.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrazyLizard.Interfaces.Repository;

namespace CrazyLizard.Repositories
{
    public class ProfileRepository(ApplicationDbContext ctx) : IProfileRepository
    {

        public async Task BlockUserAsync(long blockerId, long blockedId, DateTimeOffset time)
        {
            Block toAdd = new()
            {
                BlockerId = blockerId,
                BlockedId = blockedId,
                BlockDate = time,
            };

            ctx.Blocks.Add(toAdd);

            await ctx.SaveChangesAsync();
        }

        public async Task UnblockUserAsync(long blockerId, long blockedId)
        {
            await ctx.Blocks.
            Where(b => b.BlockerId == blockerId && b.BlockedId == blockedId).
            ExecuteDeleteAsync();
        }

        public async Task<List<User>> GetBlockedUsersAsync(long id)
        {
            return await
            ctx.Blocks.
            Where(l => l.BlockerId == id).
            Join(
                ctx.Users,
                l => l.BlockedId,
                u => u.Id,
                (l, u) => u
            ).
            ToListAsync();
        }

        public async Task<List<User>> GetUsersBlockingAsync(long userId)
        {
            return await
            ctx.Blocks.Where(l => l.BlockedId == userId).
            Join(ctx.Users,
            l => l.BlockerId,
            u => u.Id,
            (l, u) => u).
            ToListAsync();
        }

        public async Task<DateTimeOffset> BlockedSince(long userId, long targetId)
        {
            return await ctx.Blocks.
                   Where(l => l.BlockerId == userId && l.BlockedId == targetId).
                   Select(l => l.BlockDate).
                   SingleAsync();
        }
    }
}
