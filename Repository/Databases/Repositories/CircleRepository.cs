using Microsoft.EntityFrameworkCore;
using Repository.Databases.Contexts;
using Repository.Databases.Entities;

namespace Repository.Databases.Stores
{
    public class CircleRepository : Repository, ICircleDatabase
    {

        internal CircleRepository(Func<CardinalContext> contextFactory) : base(contextFactory)
        {
        }

        private async Task<string> GenerateUniqueCircleCodeAsync(CardinalContext ctx)
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
            await using CardinalContext ctx = initContext();

            return await ctx.Circles.
                   Where(c => c.Id == circleId).
                   Select(c => new CoreCircle(c.Id, c.CircleCode, c.Title, c.TimeOfCreation, c.Plan, c.IssueSchedule, c.SoftDeleted)).
                   SingleAsync();
        }

        public async Task<CoreCircle> GetCircleByCodeAsync(string circleCode)
        {
            await using CardinalContext ctx = initContext();

            return await ctx.Circles.
                   Where(c => c.CircleCode == circleCode).
                   Select(c => new CoreCircle(c.Id, c.CircleCode, c.Title, c.TimeOfCreation, c.Plan, c.IssueSchedule, c.SoftDeleted)).
                   SingleAsync();
        }

        public async Task<List<CoreCircle>> GetCirclesForUserAsync(long userId)
        {
            await using CardinalContext ctx = initContext();

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
            await using CardinalContext ctx = initContext();
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
                    JoinDate = DateTimeOffset.UtcNow,
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
            await using CardinalContext ctx = initContext();

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
            await using CardinalContext ctx = initContext();

            Circle c = new() { Id = circleId, CircleCode = await GenerateUniqueCircleCodeAsync(ctx) };

            ctx.Circles.Attach(c);
            ctx.Entry(c).Property(nameof(c.CircleCode)).IsModified = true;
            await ctx.SaveChangesAsync();

            return c.CircleCode;
        }

        public async Task DeleteCircleAsync(long circleId)
        {
            await using CardinalContext ctx = initContext();
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                await ctx.CircleRecipients.Where(cr => cr.CircleId == circleId).ExecuteDeleteAsync();
                await ctx.CircleMemberships.Where(m => m.CircleId == circleId).ExecuteDeleteAsync();

                List<long> issuesToDelete = await ctx.Issues.Where(i => i.CircleId == circleId).Select(i => i.Id).ToListAsync();
                List<long> postsToDelete = await ctx.Posts.Where(p => issuesToDelete.Contains(p.IssueId)).Select(i => i.Id).ToListAsync();
                List<long> snapshotsToDelete = await ctx.Snapshots.Where(s => postsToDelete.Contains(s.PostId)).Select(i => i.Id).ToListAsync();
                List<long> captionsToDelete = await ctx.Captions.Where(c => postsToDelete.Contains(c.PostId)).Select(i => i.Id).ToListAsync();

                await ctx.Captions.Where(c => captionsToDelete.Contains(c.Id)).ExecuteDeleteAsync();
                await ctx.Snapshots.Where(s => snapshotsToDelete.Contains(s.Id)).ExecuteDeleteAsync();
                await ctx.Posts.Where(p => postsToDelete.Contains(p.Id)).ExecuteDeleteAsync();
                await ctx.Issues.Where(i => issuesToDelete.Contains(i.Id)).ExecuteDeleteAsync();

                ctx.Circles.Remove(new() { Id = circleId });
                await ctx.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<CoreCircleMembership>> GetCircleMembersAsync(long circleId)
        {
            await using CardinalContext ctx = initContext();

            return await ctx.CircleMemberships.
                   Where(m => m.CircleId == circleId).
                   Select(m => new CoreCircleMembership(m.UserId, m.JoinDate, m.Type)).
                   ToListAsync();
        }

        public Task<List<RecipientShard>> GetRecipientsForCircleAsync(long circleId)
        {
            throw new NotImplementedException();
        }

        public async Task<CoreCircleMembership> GetCircleMembershipAsync(long userId, long circleId)
        {
            await using CardinalContext ctx = initContext();

            return await ctx.CircleMemberships.
                   Where(m => m.UserId == userId && m.CircleId == circleId).
                   Select(m => new CoreCircleMembership(m.UserId, m.JoinDate, m.Type)).
                   SingleAsync();
        }

        public async Task UpdateCircleMemberAsync(long userId, long circleId, List<(string Property, object Value)> edits)
        {
            await using CardinalContext ctx = initContext();

            Circle c = new() { Id = circleId };
            ctx.Circles.Attach(c);

            foreach ((string Property, object Value) in edits)
            {
                switch (Property)
                {
                    case nameof(CoreRecipient.):
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

        public async Task AddCircleMemberAsync(long userId, long circleId)
        {
            await using CardinalContext ctx = initContext();

            CircleMembership toAdd = new() 
            {
                UserId = userId,
                CircleId = circleId,
                JoinDate = DateTimeOffset.UtcNow,
                Type = CircleMembershipType.Regular,
            };

            ctx.CircleMemberships.Add(toAdd);
            await ctx.SaveChangesAsync();
        }

        public async Task RemoveCircleMembershipAsync(long userId, long circleId)
        {
            await using CardinalContext ctx = initContext();

            long id = await ctx.CircleMemberships.
                      Where(m => m.UserId == userId && m.CircleId == circleId).
                      Select(m => m.Id).
                      SingleAsync();

            ctx.CircleMemberships.Remove(new() { Id = id });
            await ctx.SaveChangesAsync();
        }

        public async Task UpdateRecipientAsync(long recipientId, List<(string Property, object Value)> edits)
        {
            await using CardinalContext ctx = initContext();

            Recipient r = new() { Id = recipientId };
            ctx.Recipients.Attach(r);

            foreach ((string Property, object Value) in edits)
            {
                switch (Property)
                {
                    case nameof(CoreRecipient.Title):
                        r.Title = (string)Value;
                        break;
                    case nameof(CoreRecipient.FirstName):
                        r.FirstName = (string)Value;
                        break;
                    case nameof(CoreRecipient.LastName):
                        r.LastName = (string)Value;
                        break;
                    case nameof(CoreRecipient.Address.ApartmentOrSuite):
                        r.UnitNumber = (string)Value;
                        break;
                    case nameof(CoreRecipient.Address.Street):
                        r.StreetAddress = (string)Value;
                        break;
                    case nameof(CoreRecipient.Address.City):
                        r.City = (string)Value;
                        break;
                    case nameof(CoreRecipient.Address.ProvinceOrState):
                        r.ProvinceOrState = (string)Value;
                        break;
                    case nameof(CoreRecipient.Address.PostalCode):
                        r.PostalCode = (string)Value;
                        break;
                    case nameof(CoreRecipient.Address.Country):
                        r.Country = (string)Value;
                        break;
                    case nameof(CoreRecipient.DateOfBirth):
                        r.DateOfBirth = (DateTimeOffset)Value;
                        break;
                    case nameof(CoreRecipient.ManagerId):
                        r.ManagerId = (long)Value;
                        break;
                    default:
                        throw new InvalidInputException($"Property named \"{Property}\" can not be updated using this method.");
                }
                ctx.Entry(r).Property(Property).IsModified = true;
            }
            await ctx.SaveChangesAsync();
        }

        public async Task DeleteRecipientAsync(long recipientId)
        {
            await using CardinalContext ctx = initContext();
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                await ctx.CircleRecipients.Where(cr => cr.RecipientId == recipientId).ExecuteDeleteAsync();
                
                ctx.Recipients.Remove(new() { Id = recipientId });
                await ctx.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task AddRecipientAsync(long circleId, string title, string firstName, string lastName, string streetAddress, string city, string provinceOrState, string postalCode, string country)
        {
            await using CardinalContext ctx = initContext();
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                Recipient toAdd = new()
                {
                    Title = title,
                    FirstName = firstName,
                    LastName = lastName,
                    StreetAddress = streetAddress,
                    City = city,
                    ProvinceOrState = provinceOrState,
                    PostalCode = postalCode,
                    Country = country,
                };

                ctx.Recipients.Add(toAdd);
                await ctx.SaveChangesAsync();

                CircleRecipient link = new()
                {
                    CircleId = circleId,
                    RecipientId = toAdd.Id,
                    JoinDate = DateTimeOffset.UtcNow,
                };

                ctx.CircleRecipients.Add(link);
                await ctx.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}

