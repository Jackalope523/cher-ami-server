using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
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

        public async Task<User> GetUserAsync(long userId, CancellationToken cancellationToken = default)
        {
            return await ctx.Users
                .Where(x => x.Id == userId)
                .SingleAsync(cancellationToken: cancellationToken);
        }

        public async Task<User> GetUserWithRecipientsAsync(long userId, CancellationToken cancellationToken = default)
        {
            return await ctx.Users
                .Where(x => x.Id == userId)
                .Include(x => x.Recipients)
                .SingleAsync(cancellationToken: cancellationToken);
        }

        public async Task<User> FindUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await ctx.Users
                .Where(x => x.Email == email)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }

        public async Task<List<User>> GetBlockedUsers(long userId, CancellationToken cancellationToken = default)
        {
            return await ctx.Blocks
                .Where(x => x.BlockerId == userId)
                .Select(x => x.Blocked)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<List<long>> GetBlacklistedUserIdsAsync(long userId, CancellationToken cancellationToken = default)
        {
            List<long> blockedIds = await ctx.Blocks
                .Where(x => x.BlockerId == userId)
                .Select(x => x.BlockedId)
                .ToListAsync(cancellationToken: cancellationToken);

            List<long> blockedByIds = await ctx.Blocks
                .Where(x => x.BlockedId == userId)
                .Select(x => x.BlockerId)
                .ToListAsync(cancellationToken: cancellationToken);

            return [.. blockedIds, .. blockedByIds];
        }

        public async Task SaveUserAsync(User user, CancellationToken cancellationToken = default)
        {
            await ctx.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> AnyUsersAsync(CancellationToken cancellationToken = default)
        {
            return await ctx.Users.AnyAsync(cancellationToken: cancellationToken);
        }

        public async Task UpdateProfileAsync(long userId, string firstName, string lastName, string avatarPath = null, DateTimeOffset? avatarTimestamp = null, CancellationToken cancellationToken = default)
        {
            User user = await ctx.Users.Where(x => x.Id == userId).SingleAsync(cancellationToken: cancellationToken);

            user.FirstName = firstName;
            user.LastName = lastName;

            if (avatarPath != null)
            {
                user.AvatarPath = avatarPath;
                user.AvatarTimestamp = avatarTimestamp;
            }

            await ctx.SaveChangesAsync(cancellationToken);
        }

        public async Task SetAvatarAsync(long userId, string avatarPath, DateTimeOffset avatarTimestamp, CancellationToken cancellationToken = default)
        {
            User user = await ctx.Users.Where(x => x.Id == userId).SingleAsync(cancellationToken: cancellationToken);

            user.AvatarPath = avatarPath;
            user.AvatarTimestamp = avatarTimestamp;

            await ctx.SaveChangesAsync(cancellationToken);
        }

        public async Task<string> GetAvatarPathAsync(long userId, CancellationToken cancellationToken = default)
        {
            return await ctx.Users
                .Where(x => x.Id == userId)
                .Select(x => x.AvatarPath)
                .SingleAsync(cancellationToken: cancellationToken);
        }

        public async Task<bool> HasBlockedAsync(long blockerId, long blockedId, CancellationToken cancellationToken = default)
        {
            return await ctx.Blocks.AnyAsync(x => x.BlockerId == blockerId && x.BlockedId == blockedId, cancellationToken);
        }

        public async Task CreateBlockAsync(long blockerId, long blockedId, CancellationToken cancellationToken = default)
        {
            Block block = new()
            {
                BlockerId = blockerId,
                BlockedId = blockedId,
                BlockDate = DateTimeOffset.UtcNow
            };

            ctx.Blocks.Add(block);
            await ctx.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> RemoveBlockAsync(long blockerId, long blockedId, CancellationToken cancellationToken = default)
        {
            int rowsDeleted = await ctx.Blocks
                .Where(x => x.BlockerId == blockerId && x.BlockedId == blockedId)
                .ExecuteDeleteAsync(cancellationToken: cancellationToken);

            return rowsDeleted > 0;
        }

        public async Task PurgeUserDataAsync(long userId, CancellationToken cancellationToken = default)
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await ctx.Reports.Where(x => x.FilingUserId == userId).ExecuteDeleteAsync(cancellationToken);
                await ctx.UserReports.Where(x => x.ReportedUserId == userId).ExecuteDeleteAsync(cancellationToken);
                await ctx.Posts.Where(x => x.AuthorId == userId).ExecuteDeleteAsync(cancellationToken);
                await ctx.Recipients.Where(x => x.ManagerId == userId).ExecuteDeleteAsync(cancellationToken);
                await ctx.Users.Where(x => x.Id == userId).ExecuteDeleteAsync(cancellationToken);

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
