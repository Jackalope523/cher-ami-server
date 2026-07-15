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
    public class CircleRepository(ApplicationDbContext ctx) : ICircleRepository
    {
        public async Task<long?> GetCircleIdOfUserAsync(long userId, CancellationToken cancellationToken = default)
        {
            return await ctx.Users
                .Where(x => x.Id == userId)
                .Select(x => x.CircleId)
                .SingleAsync(cancellationToken: cancellationToken);
        }

        public async Task<Circle> GetCircleOfUserAsync(long userId, CancellationToken cancellationToken = default)
        {
            return await ctx.Users
                .Where(x => x.Id == userId)
                .Select(x => x.Circle)
                .SingleAsync(cancellationToken: cancellationToken);
        }

        public async Task<Circle> GetCircleWithContributorsAsync(long circleId, List<long> excludedUserIds, CancellationToken cancellationToken = default)
        {
            return await ctx.Circles
                .Where(x => x.Id == circleId)
                .Include(x => x.Contributors.Where(x => !excludedUserIds.Contains(x.Id)))
                .ThenInclude(x => x.Recipients.Where(x => !excludedUserIds.Contains(x.ManagerId)))
                .SingleAsync(cancellationToken: cancellationToken);
        }

        public async Task<string> GetCircleCodeOfUserAsync(long userId, CancellationToken cancellationToken = default)
        {
            return await ctx.Users
                .Where(x => x.Id == userId)
                .Select(x => x.Circle.CircleCode)
                .SingleAsync(cancellationToken: cancellationToken);
        }

        public async Task<long> GetCircleIdByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await ctx.Circles
                .Where(x => x.CircleCode == code)
                .Select(x => x.Id)
                .SingleOrDefaultAsync(cancellationToken: cancellationToken);
        }

        public async Task<bool> IsUserInCircleAsync(long userId, long circleId, CancellationToken cancellationToken = default)
        {
            return await ctx.Users.AnyAsync(u => u.Id == userId && u.CircleId == circleId, cancellationToken: cancellationToken);
        }

        public async Task<string> GetHeaderPathAsync(long circleId, CancellationToken cancellationToken = default)
        {
            return await ctx.Circles
                .Where(x => x.Id == circleId)
                .Select(x => x.HeaderPath)
                .SingleAsync(cancellationToken: cancellationToken);
        }

        public async Task CreateCircleAsync(Circle circle, Issue firstIssue, long ownerId, CancellationToken cancellationToken = default)
        {
            User owner = await ctx.Users.Where(x => x.Id == ownerId).SingleAsync(cancellationToken: cancellationToken);

            circle.Issues.Add(firstIssue);
            ctx.Circles.Add(circle);

            owner.Circle = circle;
            owner.CircleJoinDate = DateTimeOffset.UtcNow;

            // Single SaveChanges so the circle, its first issue, and the owner's membership commit atomically
            await ctx.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateCircleAsync(long circleId, string title, string headerPath = null, DateTimeOffset? headerTimestamp = null, CancellationToken cancellationToken = default)
        {
            Circle circle = await ctx.Circles.Where(x => x.Id == circleId).SingleAsync(cancellationToken: cancellationToken);

            circle.Title = title;

            if (headerPath != null)
            {
                circle.HeaderPath = headerPath;
                circle.HeaderTimestamp = headerTimestamp.Value;
            }

            await ctx.SaveChangesAsync(cancellationToken);
        }

        public async Task SetHeaderAsync(long circleId, string headerPath, DateTimeOffset headerTimestamp, CancellationToken cancellationToken = default)
        {
            Circle circle = await ctx.Circles.Where(x => x.Id == circleId).SingleAsync(cancellationToken: cancellationToken);

            circle.HeaderPath = headerPath;
            circle.HeaderTimestamp = headerTimestamp;

            await ctx.SaveChangesAsync(cancellationToken);
        }

        public async Task SetCircleCodeAsync(long circleId, string code, CancellationToken cancellationToken = default)
        {
            Circle circle = await ctx.Circles.Where(x => x.Id == circleId).SingleAsync(cancellationToken: cancellationToken);

            circle.CircleCode = code;

            await ctx.SaveChangesAsync(cancellationToken);
        }

        public async Task AddUserToCircleAsync(long userId, long circleId, CancellationToken cancellationToken = default)
        {
            User user = await ctx.Users.Where(x => x.Id == userId).SingleAsync(cancellationToken: cancellationToken);

            user.CircleId = circleId;
            user.CircleJoinDate = DateTimeOffset.UtcNow;

            await ctx.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveUserFromCircleAsync(long userId, CancellationToken cancellationToken = default)
        {
            User user = await ctx.Users.Where(x => x.Id == userId).SingleAsync(cancellationToken: cancellationToken);

            user.CircleId = null;
            user.CircleJoinDate = null;

            await ctx.SaveChangesAsync(cancellationToken);
        }
    }
}
