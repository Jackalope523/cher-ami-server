using Core.Boundaries;
using CrazyLizard.Contexts;
using Microsoft.EntityFrameworkCore;
using CrazyLizard.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Circle = CrazyLizard.Entities.Circle;

namespace CrazyLizard.Repositories
{
    public class CircleRepository(ApplicationDbContext ctx, IIssueRepository issueRepository) : ICircleRepository
    {

        private async Task<string> GenerateUniqueCircleCodeAsync(ApplicationDbContext ctx)
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
                   Select(c => new CoreCircle(c.Id, c.CircleCode, c.Title, c.TimeOfCreation, c.IssueSchedule, c.SoftDeleted)).
                   SingleOrDefaultAsync();
        }

        public async Task<CoreCircle> GetCircleByCodeAsync(string circleCode)
        {
            return await ctx.Circles.
                   Where(c => c.CircleCode == circleCode).
                   Select(c => new CoreCircle(c.Id, c.CircleCode, c.Title, c.TimeOfCreation, c.IssueSchedule, c.SoftDeleted)).
                   SingleOrDefaultAsync();
        }

        public async Task<CoreCircle> GetCircleForUserAsync(long userId)
        {
            return await ctx.Users.
                   Where(x => x.Id == userId).
                   Join(
                        ctx.Circles, 
                        x => x.CircleId,
                        y => y.Id,
                        (x,y) => new CoreCircle(y.Id, y.CircleCode, y.Title, y.TimeOfCreation, y.IssueSchedule, y.SoftDeleted)
                   ).SingleOrDefaultAsync();
        }

        public async Task<CoreCircle> CreateCircleAsync(long founderId, string title, IssueSchedule schedule)
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
                    IssueSchedule = schedule
                };

                ctx.Circles.Add(toCreate);
                await ctx.SaveChangesAsync();

                User founder = await ctx.Users.FindAsync(founderId);
                founder.CircleId = toCreate.Id;
                founder.CircleJoinDate = DateTimeOffset.UtcNow;
                await ctx.SaveChangesAsync();

                await issueRepository.CreateIssue(toCreate.Id);

                await transaction.CommitAsync();

                return new CoreCircle
                (
                    toCreate.Id, 
                    toCreate.CircleCode, 
                    toCreate.Title, 
                    toCreate.TimeOfCreation, 
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
                await ctx.Users.Where(x => x.CircleId == circleId).ExecuteUpdateAsync(setters => setters.SetProperty(u => u.CircleId, (long?)null));

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

        public async Task<List<User>> GetCircleContributorsAsync(long circleId)
        {
            return await ctx.Users.Where(m => m.CircleId == circleId).ToListAsync();
        }

        public async Task<List<CoreRecipient>> GetRecipientsForCircleAsync(long circleId)
        {
            List<long> circleContributorIds = await ctx.Users.
                                              Where(m => m.CircleId == circleId).
                                              Select(y => y.Id).
                                              ToListAsync();

            return await ctx.Recipients.
                   Where(x => circleContributorIds.Contains(x.ManagerId)).
                   Select
                   (
                      r => new CoreRecipient
                      (
                          r.Id,
                          r.ManagerId,
                          r.Title,
                          r.FirstName,
                          r.LastName,
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

            User user = ctx.Users.Find(userId);
            user.CircleId = circleId;
            await ctx.SaveChangesAsync();
        }

        public async Task RemoveCircleMembershipAsync(long userId)
        {
            User user = ctx.Users.Find(userId);
            user.CircleId = null;
            await ctx.SaveChangesAsync();
        }

        public async Task<bool> IsMemberAsync(long userId, long circleId)
        {
            return await ctx.Users.AnyAsync(x => x.Id == userId && x.CircleId == circleId);
        }

        public async Task<bool> HasCircle(long userId)
        {
            return await ctx.Users.AnyAsync(x => x.Id == userId && x.CircleId != null);
        }

        public Task<bool> Exists(long circleId)
        {
            return ctx.Circles.AnyAsync(x => x.Id == circleId);
        }

        public Task<bool> Exists(string circleCode)
        {
            return ctx.Circles.AnyAsync(x => x.CircleCode == circleCode);
        }
    }
}

