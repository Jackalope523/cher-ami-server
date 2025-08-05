using Microsoft.EntityFrameworkCore;
using Repository.Contexts;
using Repository.Entities;

namespace Repository.Repositories
{
    public class ProfileRepository : Repository, IProfileRepository
    {
        internal ProfileRepository(Func<LLContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task BlockUserAsync(long blockerId, long blockedId, DateTimeOffset time)
        {
            await using LLContext ctx = initContext();

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
            await using LLContext ctx = initContext();

            await ctx.Blocks.
            Where(b => b.BlockerId == blockerId && b.BlockedId == blockedId).
            ExecuteDeleteAsync();
        }

        public async Task<List<BlockedUserShard>> GetBlockedUsersAsync(long id)
        {
            await using LLContext ctx = initContext();

            return await
            ctx.Blocks.
            Where(l => l.BlockerId == id).
            Join(
                ctx.Users,
                l => l.BlockedId,
                u => u.Id,
                (l, u) => new BlockedUserShard(u.Id, u.FirstName, l.BlockDate)
            ).
            ToListAsync();
        }

        public async Task<List<CoreUser>> GetUsersBlockingAsync(long userId)
        {
            await using LLContext ctx = initContext();

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
            await using LLContext ctx = initContext();

            return await ctx.Blocks.
                   Where(l => l.BlockerId == userId && l.BlockedId == targetId).
                   Select(l => l.BlockDate).
                   SingleAsync();
        }
    }
}
