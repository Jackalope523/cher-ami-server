using Core.Boundaries;
using CrazyLizard.Contexts;
using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CrazyLizard.Repositories
{
    public class ProfileRepository(CrazyLizardContext ctx) : IProfileRepository
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

        public async Task<List<CoreBlockedUser>> GetBlockedUsersAsync(long id)
        {
            return await
            ctx.Blocks.
            Where(l => l.BlockerId == id).
            Join(
                ctx.Users,
                l => l.BlockedId,
                u => u.Id,
                (l, u) => new CoreBlockedUser(u.Id, $"{u.FirstName} {u.LastName}", l.BlockDate)
            ).
            ToListAsync();
        }

        public async Task<List<CoreUser>> GetUsersBlockingAsync(long userId)
        {
            return await
            ctx.Blocks.Where(l => l.BlockedId == userId).
            Join(ctx.Users,
            l => l.BlockerId,
            u => u.Id,
            (l, u) => new CoreUser(
                  u.Id,
                  u.PhoneNumber,
                  u.Email,
                  u.NormalizedEmail,
                  u.Title,
                  u.FirstName,
                  u.LastName,
                  u.DateOfBirth,
                  u.IsPhoneConfirmed,
                  u.IsEmailConfirmed,
                  u.SoftDeleted,
                  u.SecurityStamp,
                  u.LockoutDate,
                  u.AccessTries,
                  u.AccountStatus,
                  u.JoinDate,
                  u.TimeOfUserAgreement,
                  u.NotificationId
                  )
            ).
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
