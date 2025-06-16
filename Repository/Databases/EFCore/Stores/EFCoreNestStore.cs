using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class EFCoreNestStore : QueryStore, INestDatabase
    {
        private static readonly Func<CanaryContext, long, long, UserRelationship.UserRelationshipType, Task> RemoveLinkOperation =
            EF.CompileAsyncQuery(
                (CanaryContext ctx, long selfId, long otherId, UserRelationship.UserRelationshipType type) =>
                ctx.UserRelationships
                .Where(l => l.SelfId == selfId && l.OtherId == otherId && l.Type == type)
                .ExecuteDelete());

        public EFCoreNestStore(Harbor.Flag flag) : base(flag)
        {

        }

        public async Task FollowUserAsync(long selfId, long targetId, DateTimeOffset time)
        {
            long id = await storeSentry.ExecuteReadAsync(ctx =>
                ctx.UserRelationships.
                Where(l => l.SelfId == selfId && l.OtherId == targetId)
                .Select(l => l.Id)
                .SingleOrDefaultAsync());


            if (id == 0)
            {
                UserRelationship toAdd = new()
                {
                    SelfId = selfId,
                    OtherId = targetId,
                    Time = time,
                    Type = UserRelationship.UserRelationshipType.Follow
                };

                await storeSentry.ExecuteWriteAsync(ctx => ctx.UserRelationships.Add(toAdd));
            }
            else
            {
                UserRelationship toUpdate = new()
                {
                    Id = id,
                    SelfId = selfId,
                    OtherId = targetId,
                    Time = time,
                    Type = UserRelationship.UserRelationshipType.Follow
                };

                await storeSentry.ExecuteWriteAsync(ctx => ctx.UserRelationships.Update(toUpdate));
            }
        }
        public async Task UnfollowUserAsync(long selfId, long targetId)
        {
            await storeSentry.ExecuteWriteAsync(ctx =>
            RemoveLinkOperation(ctx, selfId, targetId, UserRelationship.UserRelationshipType.Follow));
        }
        public async Task BlockUserAsync(long selfId, long targetId, DateTimeOffset time)
        {
            long id = await storeSentry.ExecuteReadAsync(ctx =>
               ctx.UserRelationships.
               Where(l => l.SelfId == selfId && l.OtherId == targetId)
               .Select(l => l.Id)
               .SingleOrDefaultAsync());


            if (id == 0)
            {
                UserRelationship toAdd = new()
                {
                    SelfId = selfId,
                    OtherId = targetId,
                    Time = time,
                    Type = UserRelationship.UserRelationshipType.Block
                };

                await storeSentry.ExecuteWriteAsync(ctx => ctx.UserRelationships.Add(toAdd));
            }
            else
            {
                UserRelationship toUpdate = new()
                {
                    Id = id,
                    SelfId = selfId,
                    OtherId = targetId,
                    Time = time,
                    Type = UserRelationship.UserRelationshipType.Block
                };

                await storeSentry.ExecuteWriteAsync(ctx => ctx.UserRelationships.Update(toUpdate));
            }
        }
        public async Task UnblockUserAsync(long selfId, long targetId)
        {
            await storeSentry.ExecuteWriteAsync(ctx =>
            RemoveLinkOperation(ctx, selfId, targetId, UserRelationship.UserRelationshipType.Block));
        }

        public async Task<List<CoreUser>> GetFollowedUsersAsync(long id)
        {
            return await storeSentry.ExecuteReadAsync(ctx =>
             ctx.UserRelationships.Where(l => l.SelfId == id && l.Type == UserRelationship.UserRelationshipType.Follow).
             Join(
                 ctx.Users,
                 l => l.OtherId,
                 u => u.Id,
                 (l, u) => new CoreUser(u.Id,
                      u.PhoneNumber,
                      u.Email,
                      u.Name,
                      u.CompanionshipCode,
                      u.DateOfBirth,
                      u.IsPhoneConfirmed,
                      u.IsEmailConfirmed,
                      u.SoftDeleted,
                      u.SecurityStamp,
                      u.LockoutDate,
                      u.AccessTries,
                      u.AccountStatus,
                      u.JoinDate,
                      u.Reputation,
                      new CharacterShard(
                          u.Age,
                          u.Extroversion,
                          u.Athleticisme,
                          u.Chaos,
                          u.Competitiveness,
                          u.Industriousness,
                          u.NightOwl,
                          u.Openness),
                      u.TimeOfUserAgreement,
                      u.NotificationId
                  )
                 ).
             ToListAsync());
        }
        public async Task<List<BlockedUserShard>> GetBlockedUsersAsync(long id)
        {
            return await storeSentry.ExecuteReadAsync(ctx =>
            ctx.UserRelationships.Where(l => l.SelfId == id && l.Type == UserRelationship.UserRelationshipType.Block).
            Join(
                ctx.Users,
                l => l.OtherId,
                u => u.Id,
                (l, u) => new BlockedUserShard(u.Id, u.Name, l.Time)
                ).
            ToListAsync());
        }
        public async Task<List<CoreUser>> GetCompanionsAsync(long id)
        {
            Task<List<CoreUser>> appreciating = storeSentry.ExecuteReadAsync(ctx =>
             ctx.UserRelationships.Where(l => l.SelfId == id && l.Type == UserRelationship.UserRelationshipType.Follow).
             Join(
                 ctx.Users,
                 l => l.OtherId,
                 u => u.Id,
                 (l, u) => new CoreUser(u.Id,
                      u.PhoneNumber,
                      u.Email,
                      u.Name,
                      u.CompanionshipCode,
                      u.DateOfBirth,
                      u.IsPhoneConfirmed,
                      u.IsEmailConfirmed,
                      u.SoftDeleted,
                      u.SecurityStamp,
                      u.LockoutDate,
                      u.AccessTries,
                      u.AccountStatus,
                      u.JoinDate,
                      u.Reputation,
                      new CharacterShard(
                          u.Age,
                          u.Extroversion,
                          u.Athleticisme,
                          u.Chaos,
                          u.Competitiveness,
                          u.Industriousness,
                          u.NightOwl,
                          u.Openness),
                      u.TimeOfUserAgreement,
                      u.NotificationId
                  )
                 ).
             ToListAsync());

            Task<List<CoreUser>> appreciatingMe = storeSentry.ExecuteReadAsync(ctx =>
             ctx.UserRelationships.Where(l => l.OtherId == id && l.Type == UserRelationship.UserRelationshipType.Follow).
             Join(
                 ctx.Users,
                 l => l.SelfId,
                 u => u.Id,
                 (l, u) => new CoreUser(u.Id,
                      u.PhoneNumber,
                      u.Email,
                      u.Name,
                      u.CompanionshipCode,
                      u.DateOfBirth,
                      u.IsPhoneConfirmed,
                      u.IsEmailConfirmed,
                      u.SoftDeleted,
                      u.SecurityStamp,
                      u.LockoutDate,
                      u.AccessTries,
                      u.AccountStatus,
                      u.JoinDate,
                      u.Reputation,
                      new CharacterShard(
                          u.Age,
                          u.Extroversion,
                          u.Athleticisme,
                          u.Chaos,
                          u.Competitiveness,
                          u.Industriousness,
                          u.NightOwl,
                          u.Openness),
                      u.TimeOfUserAgreement,
                      u.NotificationId
                  )
                 ).
             ToListAsync());

            return (await appreciating).Intersect(await appreciatingMe).ToList();
        }

        public async Task<List<CoreUser>> GetUserFollowersAsync(long userId)
        {
            return await storeSentry.ExecuteReadAsync(ctx =>
            ctx.UserRelationships.Where(l => l.OtherId == userId && l.Type == UserRelationship.UserRelationshipType.Follow).
            Join(ctx.Users,
            l => l.SelfId,
            u => u.Id,
            (l, u) => new CoreUser(u.Id,
                      u.PhoneNumber,
                      u.Email,
                      u.Name,
                      u.CompanionshipCode,
                      u.DateOfBirth,
                      u.IsPhoneConfirmed,
                      u.IsEmailConfirmed,
                      u.SoftDeleted,
                      u.SecurityStamp,
                      u.LockoutDate,
                      u.AccessTries,
                      u.AccountStatus,
                      u.JoinDate,
                      u.Reputation,
                      new CharacterShard(
                          u.Age,
                          u.Extroversion,
                          u.Athleticisme,
                          u.Chaos,
                          u.Competitiveness,
                          u.Industriousness,
                          u.NightOwl,
                          u.Openness),
                      u.TimeOfUserAgreement,
                      u.NotificationId
                  )).
            ToListAsync());
        }

        public async Task<List<CoreUser>> GetUsersBlockingAsync(long userId)
        {
            return await storeSentry.ExecuteReadAsync(ctx =>
            ctx.UserRelationships.Where(l => l.OtherId == userId && l.Type == UserRelationship.UserRelationshipType.Block).
            Join(ctx.Users,
            l => l.SelfId,
            u => u.Id,
            (l, u) => new CoreUser(u.Id,
                      u.PhoneNumber,
                      u.Email,
                      u.Name,
                      u.CompanionshipCode,
                      u.DateOfBirth,
                      u.IsPhoneConfirmed,
                      u.IsEmailConfirmed,
                      u.SoftDeleted,
                      u.SecurityStamp,
                      u.LockoutDate,
                      u.AccessTries,
                      u.AccountStatus,
                      u.JoinDate,
                      u.Reputation,
                      new CharacterShard(
                          u.Age,
                          u.Extroversion,
                          u.Athleticisme,
                          u.Chaos,
                          u.Competitiveness,
                          u.Industriousness,
                          u.NightOwl,
                          u.Openness),
                      u.TimeOfUserAgreement,
                      u.NotificationId
                  )).
            ToListAsync());
        }

        public Task<bool> HaveMutualGathering(long userId, long targetId)
        {
            return storeSentry.ExecuteReadAsync(ctx =>
                ctx.GatheringLinks.
                Where(l => l.UserId == userId && l.Type == GatheringBond.Guest).
                Join(
                      ctx.GatheringLinks.Where(l => l.UserId == targetId && l.Type == GatheringBond.Guest),
                      x => x.GatheringId,
                      y => y.GatheringId,
                      (x, y) => x.GatheringId
                    ).
                AnyAsync());
        }

        public async Task<CoreGathering> GetFirstMutualGathering(long userId, long targetId)
        {
            List<long> a = await storeSentry.ExecuteReadAsync(ctx => ctx.GatheringLinks.
                Where(l => l.UserId == userId && l.Type == GatheringBond.Guest).
                Select(l => l.GatheringId).
                ToListAsync());

            List<long> b = await storeSentry.ExecuteReadAsync(ctx => ctx.GatheringLinks.
                Where(l => l.UserId == targetId && l.Type == GatheringBond.Guest).
                Select(l => l.GatheringId).
                ToListAsync());

            List<long> mutualGatherings = a.Intersect(b).ToList();

            return await storeSentry.ExecuteReadAsync(ctx => ctx.Gatherings.
                Where(g => mutualGatherings.Contains(g.Id)).
                OrderByDescending(g => g.StartTime).
                GroupJoin(
                ctx.Users,
                e => e.HostId,
                u => u.Id,
                (e, users) => new { e, user = users.FirstOrDefault() }).
                Select(
                combined => new CoreGathering
                (
                    combined.e.Id,
                    combined.user != null ? combined.user.Id : 0,
                    combined.e.Title,
                    combined.e.Description,
                    combined.e.StartTime,
                    combined.e.Location.Y,
                    combined.e.Location.X,
                    combined.e.FriendlyLocation,
                    combined.e.EndTime,
                    combined.e.State,
                    combined.e.GroupMinimum,
                    combined.e.GroupMaximum,
                    new CharacterShard(
                        combined.e.Age,
                        combined.e.Extroversion,
                        combined.e.Athleticisme,
                        combined.e.Chaos,
                        combined.e.Competitiveness,
                        combined.e.Industriousness,
                        combined.e.NightOwl,
                        combined.e.Openness),
                    combined.e.Radius,
                    combined.e.IsDynamic,
                    combined.e.SoftDeleted,
                    combined.e.NumberOfGuests,
                    combined.e.DegreeOfPrivacy,
                    combined.e.Visibility,
                    combined.e.TimeOfCreation,
                    combined.e.Decay
                )).FirstAsync());
        }

        public async Task<CoreGathering> GetLatestMutualGathering(long userId, long targetId)
        {
            List<long> a = await storeSentry.ExecuteReadAsync(ctx => ctx.GatheringLinks.
               Where(l => l.UserId == userId && l.Type == GatheringBond.Guest).
               Select(l => l.GatheringId).
               ToListAsync());

            List<long> b = await storeSentry.ExecuteReadAsync(ctx => ctx.GatheringLinks.
                Where(l => l.UserId == targetId && l.Type == GatheringBond.Guest).
                Select(l => l.GatheringId).
                ToListAsync());

            List<long> mutualGatherings = a.Intersect(b).ToList();

            return await storeSentry.ExecuteReadAsync(ctx => ctx.Gatherings.
                Where(g => mutualGatherings.Contains(g.Id)).
                OrderBy(g => g.StartTime).
                GroupJoin(
                ctx.Users,
                e => e.HostId,
                u => u.Id,
                (e, users) => new { e, user = users.FirstOrDefault() }).
                Select(
                combined => new CoreGathering
                (
                    combined.e.Id,
                    combined.user != null ? combined.user.Id : 0,
                    combined.e.Title,
                    combined.e.Description,
                    combined.e.StartTime,
                    combined.e.Location.Y,
                    combined.e.Location.X,
                    combined.e.FriendlyLocation,
                    combined.e.EndTime,
                    combined.e.State,
                    combined.e.GroupMinimum,
                    combined.e.GroupMaximum,
                    new CharacterShard(
                        combined.e.Age,
                        combined.e.Extroversion,
                        combined.e.Athleticisme,
                        combined.e.Chaos,
                        combined.e.Competitiveness,
                        combined.e.Industriousness,
                        combined.e.NightOwl,
                        combined.e.Openness),
                    combined.e.Radius,
                    combined.e.IsDynamic,
                    combined.e.SoftDeleted,
                    combined.e.NumberOfGuests,
                    combined.e.DegreeOfPrivacy,
                    combined.e.Visibility,
                    combined.e.TimeOfCreation,
                    combined.e.Decay
                )).FirstAsync());
        }

        public async Task<DateTimeOffset> BlockedSince(long userId, long targetId)
        {
            return await storeSentry.ExecuteReadAsync(ctx =>
                    ctx.UserRelationships.
                    Where(l => l.SelfId == userId && l.OtherId == targetId && l.Type == UserRelationship.UserRelationshipType.Block).
                    Select(l => l.Time).
                    SingleAsync());
        }

        public async Task<List<long>> ReturnStrangerDangerAsync(long userId, params long[] users)
        {
            List<long> metUsers = await storeSentry.ExecuteReadAsync(ctx =>
                                                ctx.GatheringLinks
                                                .Where(l => l.UserId == userId)
                                                .Join(
                                                    ctx.Gatherings.Where(g => g.StartTime < DateTimeOffset.UtcNow),
                                                    l => l.GatheringId,
                                                    g => g.Id,
                                                    (l, g) => g.Id
                                                    )
                                                .Join(
                                                    ctx.GatheringLinks.Where(l => l.UserId != userId),
                                                    x => x,
                                                    l => l.GatheringId,
                                                    (x, l) => l.UserId
                                                    )
                                                .ToListAsync());

            return users.Except(metUsers).ToList();
        }

        public async Task<List<CompanionshipRequestShard>> GetIncomingRequestsAsync(long userId)
        {
            List<long> following = await storeSentry.ExecuteReadAsync(ctx =>
                                            ctx.UserRelationships
                                            .Where(l => l.SelfId == userId && l.Type == UserRelationship.UserRelationshipType.Follow)
                                            .Select(l => l.Id)
                                            .ToListAsync());

            return await storeSentry.ExecuteReadAsync(ctx => ctx.UserRelationships
            .Where( r => 
                r.OtherId == userId &&
                r.Type == UserRelationship.UserRelationshipType.Follow &&
                !following.Contains(r.Id)
            )
            .Join(
                ctx.Users,
                r => r.SelfId,
                u => u.Id,
                (r,u) => new CompanionshipRequestShard(new UserShard(u.Id, u.Name), r.Time)
            )
            .ToListAsync());
        }

        public async Task<List<CompanionshipRequestShard>> GetOutgoingRequestsAsync(long userId)
        {
            List<long> followingMe = await storeSentry.ExecuteReadAsync(ctx =>
                                         ctx.UserRelationships
                                         .Where(l => l.OtherId == userId && l.Type == UserRelationship.UserRelationshipType.Follow)
                                         .Select(l => l.Id)
                                         .ToListAsync());

            return await storeSentry.ExecuteReadAsync(ctx => ctx.UserRelationships
            .Where(r => 
                    r.SelfId == userId && 
                    r.Type == UserRelationship.UserRelationshipType.Follow &&
                    !followingMe.Contains(r.Id)
            )
            .Join(
                ctx.Users,
                r => r.OtherId,
                u => u.Id,
                (r, u) => new CompanionshipRequestShard(new UserShard(u.Id, u.Name), r.Time)
            )
            .ToListAsync());
        }

        public async Task<List<CoreUser>> GetRecentlyMetAsync(long userId)
        {
            return await storeSentry.ExecuteReadAsync(ctx =>
            ctx.GatheringLinks
            .Where(l => l.UserId == userId && l.Type == GatheringBond.Left)
            .Join(
                ctx.Gatherings.Where(g => g.StartTime < DateTimeOffset.UtcNow),
                l => l.GatheringId,
                g => g.Id,
                (l, g) => g.Id
            )
            .Join(
                ctx.GatheringLinks.Where(l => l.UserId != userId && l.Type == GatheringBond.Left),
                x => x,
                l => l.GatheringId,
                (x, l) => l.UserId
            )
            .Join(
                ctx.Users,
                id => id,
                u => u.Id,
                (id, u) => new CoreUser(
                    u.Id,
                    u.PhoneNumber,
                    u.Email,
                    u.Name,
                    u.CompanionshipCode,
                    u.DateOfBirth,
                    u.IsPhoneConfirmed,
                    u.IsEmailConfirmed,
                    u.SoftDeleted,
                    u.SecurityStamp,
                    u.LockoutDate,
                    u.AccessTries,
                    u.AccountStatus,
                    u.JoinDate,
                    u.Reputation,
                    new CharacterShard(
                        u.Age,
                        u.Extroversion,
                        u.Athleticisme,
                        u.Chaos,
                        u.Competitiveness,
                        u.Industriousness,
                        u.NightOwl,
                        u.Openness),
                    u.TimeOfUserAgreement,
                    u.NotificationId
                    )
            )
            .Take(15)
            .ToListAsync());
        }
    }
}
