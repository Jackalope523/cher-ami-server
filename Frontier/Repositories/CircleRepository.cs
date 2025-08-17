using LazyLizardBackend.Contracts.Responses;
using Microsoft.EntityFrameworkCore;
using Repository.Contexts;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Circle = Repository.Entities.Circle;

namespace Repository.Repositories
{
    public class CircleRepository(LLContext ctx) : ICircleRepository
    {

        private async Task<string> GenerateUniqueCircleCodeAsync(LLContext ctx)
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

                potentialCode = char.ToUpper(randomAdjective[0]) + randomAdjective.Substring(1) + char.ToUpper(randomNoun[0]) + randomNoun.Substring(1);

                codeUnique = !await ctx.Circles.AnyAsync(c => c.CircleCode == potentialCode);
            }

            return potentialCode;
        }

        public async Task<CoreCircle> GetCircleAsync(long circleId)
        {
            return await ctx.Circles.
                   Where(c => c.Id == circleId).
                   Select(c => new CoreCircle(c.Id, c.CircleCode, c.Title, c.TimeOfCreation, c.Plan, c.IssueSchedule, c.SoftDeleted)).
                   SingleOrDefaultAsync();
        }

        public async Task<CoreCircle> GetCircleByCodeAsync(string circleCode)
        {
            return await ctx.Circles.
                   Where(c => c.CircleCode == circleCode).
                   Select(c => new CoreCircle(c.Id, c.CircleCode, c.Title, c.TimeOfCreation, c.Plan, c.IssueSchedule, c.SoftDeleted)).
                   SingleOrDefaultAsync();
        }

        public async Task<List<CoreCircle>> GetCirclesForUserAsync(long userId)
        {
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
                        throw new ArgumentException($"Property named \"{Property}\" can not be updated using this method.");
                }
                ctx.Entry(c).Property(Property).IsModified = true;
            }
            await ctx.SaveChangesAsync();
        }

        public async Task<string> RerollCircleCode(long circleId)
        {
            Circle c = new() { Id = circleId, CircleCode = await GenerateUniqueCircleCodeAsync(ctx) };

            ctx.Circles.Attach(c);
            ctx.Entry(c).Property(nameof(c.CircleCode)).IsModified = true;
            await ctx.SaveChangesAsync();

            return c.CircleCode;
        }

        public async Task DeleteCircleAsync(long circleId)
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                await ctx.RecipientLinks.Where(cr => cr.CircleId == circleId).ExecuteDeleteAsync();
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
            return await ctx.CircleMemberships.
                   Where(m => m.CircleId == circleId).
                   Select(m => new CoreCircleMembership(m.UserId, m.JoinDate, m.Type)).
                   ToListAsync();
        }

        public async Task<List<CoreRecipient>> GetRecipientsForCircleAsync(long circleId)
        {
            return await ctx.RecipientLinks.
                   Where(cr => cr.CircleId == circleId).
                   Join
                   (
                      ctx.Recipients,
                      cr => cr.RecipientId,
                      r => r.Id,
                      (_, r) => new CoreRecipient
                      (
                          r.Id,
                          r.ManagerId,
                          r.Title,
                          r.FirstName,
                          r.LastName,
                          r.DateOfBirth,
                          new Address
                          (
                              r.StreetAddress,
                              r.UnitNumber,
                              r.City,
                              r.ProvinceOrState,
                              r.PostalCode,
                              r.Country
                          )
                      )
                   ).ToListAsync();
        }

        public async Task<CoreCircleMembership> GetCircleMembershipAsync(long userId, long circleId)
        {
            return await ctx.CircleMemberships.
                   Where(m => m.UserId == userId && m.CircleId == circleId).
                   Select(m => new CoreCircleMembership(m.UserId, m.JoinDate, m.Type)).
                   SingleAsync();
        }

        public async Task UpdateCircleMemberAsync(long userId, long circleId, List<(string Property, object Value)> edits)
        {
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
                        throw new ArgumentException($"Property named \"{Property}\" can not be updated using this method.");
                }
                ctx.Entry(c).Property(Property).IsModified = true;
            }
            await ctx.SaveChangesAsync();
        }

        public async Task AddCircleMemberAsync(long userId, string circleCode)
        {
            long circleId = await ctx.Circles.
                      Where(c => c.CircleCode == circleCode).
                      Select(c => c.Id).
                      SingleAsync();

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
            long id = await ctx.CircleMemberships.
                      Where(m => m.UserId == userId && m.CircleId == circleId).
                      Select(m => m.Id).
                      SingleAsync();

            ctx.CircleMemberships.Remove(new() { Id = id });
            await ctx.SaveChangesAsync();
        }

        public async Task UpdateRecipientAsync(long recipientId, List<(string Property, object Value)> edits)
        {
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
                        throw new ArgumentException($"Property named \"{Property}\" can not be updated using this method.");
                }
                ctx.Entry(r).Property(Property).IsModified = true;
            }
            await ctx.SaveChangesAsync();
        }

        public async Task DeleteRecipientAsync(long recipientId)
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                await ctx.RecipientLinks.Where(cr => cr.RecipientId == recipientId).ExecuteDeleteAsync();
                
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

        public async Task AddRecipientAsync(long circleId, CoreRecipient recipient)
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                Recipient toAdd = new()
                {
                    Title = recipient.Title,
                    FirstName = recipient.FirstName,
                    LastName = recipient.LastName,
                    StreetAddress = recipient.Address.Street,
                    City = recipient.Address.City,
                    ProvinceOrState = recipient.Address.ProvinceOrState,
                    PostalCode = recipient.Address.PostalCode,
                    Country = recipient.Address.Country,
                };

                ctx.Recipients.Add(toAdd);
                await ctx.SaveChangesAsync();

                RecipientLink link = new()
                {
                    CircleId = circleId,
                    RecipientId = toAdd.Id,
                    JoinDate = DateTimeOffset.UtcNow,
                };

                ctx.RecipientLinks.Add(link);
                await ctx.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task AddRecipientAsync(long circleId, long recipientId)
        {
            RecipientLink link = new()
            {
                CircleId = circleId,
                RecipientId = recipientId,
                JoinDate = DateTimeOffset.UtcNow,
            };

            ctx.RecipientLinks.Add(link);
            await ctx.SaveChangesAsync();
        }

        public async Task RemoveRecipientAsync(long circleId, long recipientId)
        {
            long id = await ctx.RecipientLinks.
                      Where(l => l.RecipientId == recipientId && l.CircleId == circleId).
                      Select(l => l.Id).
                      SingleAsync();

            ctx.RecipientLinks.Remove(new() { Id = id });
            await ctx.SaveChangesAsync();
        }

        public async Task CreateRecipient(CoreRecipient recipient)
        {
            Recipient toAdd = new()
            {
                Title = recipient.Title,
                FirstName = recipient.FirstName,
                LastName = recipient.LastName,
                StreetAddress = recipient.Address.Street,
                City = recipient.Address.City,
                ProvinceOrState = recipient.Address.ProvinceOrState,
                PostalCode = recipient.Address.PostalCode,
                Country = recipient.Address.Country,
            };

            ctx.Recipients.Add(toAdd);
            await ctx.SaveChangesAsync();
        }

        public async Task<bool> IsMemberAsync(long userId, long circleId)
        {
            return await ctx.CircleMemberships.AnyAsync(c => c.UserId == userId && c.CircleId == circleId);
        }

        public async Task<bool> IsMemberOfTypeAsync(long userId, long circleId, CircleMembershipType type)
        {
            return await ctx.CircleMemberships.AnyAsync(c => c.UserId == userId && c.CircleId == circleId && c.Type == type);
        }

        public async Task<bool> IsManagerAsync(long userId, long recipientId)
        {
            return await ctx.Recipients.AnyAsync(x => x.Id == recipientId && x.ManagerId == userId);
        }
    }
}

