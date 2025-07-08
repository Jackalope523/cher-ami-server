using Microsoft.EntityFrameworkCore;
using Repository.Databases.Contexts;
using Repository.Databases.Entities;

namespace Repository.Databases.Stores
{
    public class ProfileRepository : Repository, IProfileDatabase
    {
        internal ProfileRepository(Func<CanaryContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task BlockUserAsync(long selfId, long targetId, DateTimeOffset time)
        {
            await using CanaryContext ctx = initContext();

            long id = await ctx.UserRelationships.
                      Where(l => l.SelfId == selfId && l.OtherId == targetId).
                      Select(l => l.Id).
                      SingleOrDefaultAsync();

            UserRelationship toAddOrUpdate = new()
            {
                Id = id,
                SelfId = selfId,
                OtherId = targetId,
                Time = time,
                Type = UserRelationship.UserRelationshipType.Block
            };

            ctx.UserRelationships.Update(toAddOrUpdate);

            await ctx.SaveChangesAsync();
        }

        public async Task UnblockUserAsync(long selfId, long targetId)
        {
            await using CanaryContext ctx = initContext();

            ctx.UserRelationships.
            Where(l => l.SelfId == selfId && l.OtherId == targetId && l.Type == UserRelationship.UserRelationshipType.Block).
            ExecuteDelete();
        }

        public async Task<List<BlockedUserShard>> GetBlockedUsersAsync(long id)
        {
            await using CanaryContext ctx = initContext();

            return await
            ctx.UserRelationships.
            Where(l => l.SelfId == id && l.Type == UserRelationship.UserRelationshipType.Block).
            Join(
                ctx.Users,
                l => l.OtherId,
                u => u.Id,
                (l, u) => new BlockedUserShard(u.Id, u.FirstName, l.Time)
            ).
            ToListAsync();
        }

        public async Task<List<CoreUser>> GetUsersBlockingAsync(long userId)
        {
            await using CanaryContext ctx = initContext();

            return await
            ctx.UserRelationships.Where(l => l.OtherId == userId && l.Type == UserRelationship.UserRelationshipType.Block).
            Join(ctx.Users,
            l => l.SelfId,
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
                  )).
            ToListAsync();
        }

        public async Task<DateTimeOffset> BlockedSince(long userId, long targetId)
        {
            await using CanaryContext ctx = initContext();

            return await 
                ctx.UserRelationships.
                Where(l => l.SelfId == userId && l.OtherId == targetId && l.Type == UserRelationship.UserRelationshipType.Block).
                Select(l => l.Time).
                SingleAsync();
        }
    }
}
