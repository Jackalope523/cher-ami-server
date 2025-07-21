using Microsoft.EntityFrameworkCore;
using Repository.Databases.Contexts;
using Repository.Databases.Entities;
using Repository.Databases.Factories;
using System.Text.RegularExpressions;

namespace Repository.Databases.Stores
{
    public class CircleRepository : Repository, ICircleDatabase
    {
        internal CircleRepository(Func<CanaryContext> contextFactory) : base(contextFactory)
        {

        }

        private async Task<string> GenerateUniqueCircleCodeAsync(CanaryContext ctx)
        {
            List<string> adjectives = await ctx.Words.
                                      Where(w => w.Type == Word.WordType.Adjective).
                                      Select(w => w.Text).
                                      ToListAsync();

            List<string> nouns = await ctx.Words.
                                       Where(w => w.Type == Word.WordType.Noun).
                                       Select(w => w.Text).
                                       ToListAsync();

            bool codeUnique = false;
            Random random = new();
            string randomAdjective;
            string randomNoun;
            string potentialCode = "";

            while (!codeUnique)
            {
                randomAdjective = adjectives[random.Next(adjectives.Count)];
                randomNoun = nouns[random.Next(nouns.Count)];

                potentialCode = (Char.ToUpper(randomAdjective[0]) + randomAdjective.Substring(1)) + (Char.ToUpper(randomNoun[0]) + randomNoun.Substring(1));

                codeUnique = !(await ctx.Circles.AnyAsync(c => c.CircleCode == potentialCode));
            }

            return potentialCode;
        }

        public async Task<CoreCircle> GetCircleAsync(long circleId)
        {
            await using CanaryContext ctx = initContext();

            return await ctx.Circles.
                   Where(c => c.Id == circleId).
                   Select(c => new CoreCircle(c.Id, c.CircleCode, c.Title, c.TimeOfCreation, c.Plan, c.IssueSchedule, c.SoftDeleted)).
                   SingleAsync();
        }

        public async Task<CoreCircle> GetCircleByCodeAsync(string circleCode)
        {
            await using CanaryContext ctx = initContext();

            return await ctx.Circles.
                   Where(c => c.CircleCode == circleCode).
                   Select(c => new CoreCircle(c.Id, c.CircleCode, c.Title, c.TimeOfCreation, c.Plan, c.IssueSchedule, c.SoftDeleted)).
                   SingleAsync();
        }

        public async Task<List<CoreCircle>> GetCirclesForUserAsync(long userId)
        {
            await using CanaryContext ctx = initContext();

            return await ctx.CircleMemberships.
                   Where(c => c.UserId == userId).
                   Join(
                        ctx.Circles, 
                        m => m.CircleId,
                        c => c.Id,
                        (m,c) => new CoreCircle(c.Id, c.CircleCode, c.Title, c.TimeOfCreation, c.Plan, c.IssueSchedule, c.SoftDeleted)
                   ).ToListAsync();
        }

        public async Task<CoreCircle> CreateCircleAsync(long ownerId, string title, CirclePlan plan, IssueSchedule schedule)
        {
            await using CanaryContext ctx = initContext();
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                string code = await GenerateUniqueCircleCodeAsync(ctx);

                Circle toCreate = new()
                {
                    Title = title,
                    TimeOfCreation = DateTimeOffset.UtcNow,
                    CircleCode = code,
                    Plan = plan,
                    IssueSchedule = schedule
                };

                ctx.Circles.Add(toCreate);
                await ctx.SaveChangesAsync();

                CircleMembership ownerMembership = new()
                {
                    UserId = ownerId,
                    CircleId = toCreate.Id,
                    JoinTime = DateTimeOffset.UtcNow,
                    Type = CircleMembershipType.Owner
                };

                ctx.CircleMemberships.Add(ownerMembership);
                await ctx.SaveChangesAsync();

                await transaction.CommitAsync();

                return new CoreCircle
                (
                    toCreate.Id, 
                    toCreate.CircleCode, 
                    toCreate.Title, 
                    toCreate.TimeOfCreation, 
                    toCreate.Plan, 
                    toCreate.IssueSchedule, 
                    toCreate.SoftDeleted
                );
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateCircleAsync(long circleId, List<(string Property, object Value)> edits)
        {
            await using CanaryContext ctx = initContext();

            Circle c = new() { Id = circleId };
            ctx.Circles.Attach(c);

            foreach ((string Property, object Value) in edits)
            {
                switch (Property)
                {
                    case nameof(CoreCircle.InviteCode):
                        c.CircleCode = (string)Value;
                        break;
                    case nameof(CoreCircle.Title):
                        c.Title = (string)Value;
                        break;
                    case nameof(CoreCircle.DateCreated):
                        c.TimeOfCreation = (DateTimeOffset)Value;
                        break;
                    case nameof(CoreCircle.Plan):
                        c.Plan = (CirclePlan)Value;
                        break;
                    case nameof(CoreCircle.Schedule):
                        c.IssueSchedule = (IssueSchedule)Value;
                        break;
                    default:
                        throw new InvalidInputException($"Property named \"{Property}\" can not be updated using this method.");
                }
                ctx.Entry(c).Property(Property).IsModified = true;
            }
            await ctx.SaveChangesAsync();
        }

        public async Task<string> RerollCircleCode(long circleId)
        {
            await using CanaryContext ctx = initContext();

            Circle c = new() { Id = circleId, CircleCode = await GenerateUniqueCircleCodeAsync(ctx) };

            ctx.Circles.Attach(c);
            ctx.Entry(c).Property(nameof(c.CircleCode)).IsModified = true;
            await ctx.SaveChangesAsync();

            return c.CircleCode;
        }

        public Task DeleteCircleAsync(long circleId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<CoreCircleMembership>> GetCircleMembersAsync(long circleId)
        {
            await using CanaryContext ctx = initContext();

            return await ctx.CircleMemberships.
                   Where(m => m.CircleId == circleId).
                   Select(m => new CoreCircleMembership(m.UserId, m.JoinTime, m.Type)).
                   ToListAsync();
        }

        public Task<List<RecipientShard>> GetRecipientsForCircleAsync(long circleId)
        {
            throw new NotImplementedException();
        }

        public async Task<CoreCircleMembership> GetCircleMembershipAsync(long userId, long circleId)
        {
            await using CanaryContext ctx = initContext();

            return await ctx.CircleMemberships.
                   Where(m => m.UserId == userId && m.CircleId == circleId).
                   Select(m => new CoreCircleMembership(m.UserId, m.JoinTime, m.Type)).
                   SingleAsync();
        }

        public Task UpdateCircleMemberAsync(long userId, long circleId, List<(string Property, object Value)> edits)
        {
            throw new NotImplementedException();
        }

        public async Task AddCircleMemberAsync(long userId, long circleId)
        {
            await using CanaryContext ctx = initContext();

            CircleMembership toAdd = new() 
            {
                UserId = userId,
                CircleId = circleId,
                JoinTime = DateTimeOffset.UtcNow,
                Type = CircleMembershipType.Regular,
            };

            ctx.CircleMemberships.Add(toAdd);
            await ctx.SaveChangesAsync();
        }

        public async Task RemoveCircleMemberAsync(long userId, long circleId)
        {
            await using CanaryContext ctx = initContext();

            long id = await ctx.CircleMemberships.
                      Where(m => m.UserId == userId && m.CircleId == circleId).
                      Select(m => m.Id).
                      SingleAsync();

            ctx.CircleMemberships.Remove(new() { Id = id });
            await ctx.SaveChangesAsync();
        }

        public Task AddRecipientAsync(long circleId, long userId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRecipientAsync(long recipientId, List<(string Property, object Value)> edits)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRecipientAsync(long recipientId)
        {
            throw new NotImplementedException();
        }

        public Task SoftDeleteAsync(long circleId)
        {
            throw new NotImplementedException();
        }

        public Task HardDeleteAsync(long circleId)
        {
            throw new NotImplementedException();
        }
    }
}

