using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
using Serilog;

namespace Repository
{
    public class EFCoreAccountStore : QueryStore, IAccountDatabase
    {
        internal EFCoreAccountStore(Func<CanaryContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task<CoreUser> CreateUserAsync(string phoneNumber, string email, string normalisedEmail, string name, DateTimeOffset dateOfBirth, DateTimeOffset joinDate, CharacterShard character, Guid notificationId)
        {
            User toCreate = new()
            {
                PhoneNumber = phoneNumber,
                Email = email,
                NormalisedEmail = normalisedEmail,
                Name = name,
                DateOfBirth = dateOfBirth,
                JoinDate = joinDate,
                Extroversion = character.Extraversion,
                Athleticisme = character.Athleticism,
                Openness = character.Openness,
                Chaos = character.Chaoticness,
                Competitiveness = character.Competitiveness,
                Industriousness = character.Industriousness,
                NightOwl = character.NightOwl,
                NotificationId = notificationId,
            };

            await using(CanaryContext ctx = initContext())
            {
                ctx.Users.Add(toCreate);
                await ctx.SaveChangesAsync();
            }

            await RerollUserCodeAsync(toCreate.Id);

            return new CoreUser
              (
                  toCreate.Id,
                  toCreate.PhoneNumber,
                  toCreate.Email,
                  toCreate.Name,
                  toCreate.CompanionshipCode,
                  toCreate.DateOfBirth,
                  toCreate.IsPhoneConfirmed,
                  toCreate.IsEmailConfirmed,
                  toCreate.SoftDeleted,
                  toCreate.SecurityStamp,
                  toCreate.LockoutDate,
                  toCreate.AccessTries,
                  toCreate.AccountStatus,
                  toCreate.JoinDate,
                  toCreate.Reputation,
                  new CharacterShard(
                  toCreate.Age,
                  toCreate.Extroversion,
                  toCreate.Athleticisme,
                  toCreate.Chaos,
                  toCreate.Competitiveness,
                  toCreate.Industriousness,
                  toCreate.NightOwl,
                  toCreate.Openness),
                  toCreate.TimeOfUserAgreement,
                  toCreate.NotificationId
              );
        }

        public async Task SoftDeleteAsync(long id)
        {
            await using CanaryContext ctx = initContext();

            await ctx.ChatLinks.Where(l => l.UserId == id).ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true));
            await ctx.Connections.Where(c => c.UserId == id).ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true));
            await ctx.Notifications.Where(n => n.RecipientId == id).ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true));
            await ctx.SnapshotLinks.Where(s => s.UserId == id).ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true));
            await ctx.Snapshots.Where(s => s.OwnerId == id).ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true));
            await ctx.Telegrams.Where(t => t.NotifierId == id || t.RecipientId == id).ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true));
            await ctx.Subscriptions.Where(s => s.UserId == id).ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true));
            await ctx.Penalties.Where(p => p.PenalizedId == id).ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true));
            await ctx.GuestClearances.Where(c => c.UserId == id).ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true));
            await ctx.UserRelationships.Where(l => l.SelfId == id || l.OtherId == id).ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true));
            await ctx.GatheringLinks.Where(l => l.UserId == id).ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true));
            await ctx.Users.Where(u => u.Id == id).ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true));
        }

        public async Task HardDeleteAsync(long id)
        {
            await using CanaryContext ctx = initContext();
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                await ctx.Connections.Where(c => c.UserId == id).ExecuteDeleteAsync();
                await ctx.ChatLinks.Where(l => l.UserId == id).ExecuteDeleteAsync();
                await ctx.Notifications.Where(n => n.RecipientId == id).ExecuteDeleteAsync();
                await ctx.SnapshotLinks.Where(s => s.UserId == id).ExecuteDeleteAsync();
                await ctx.Snapshots.Where(s => s.OwnerId == id).ExecuteDeleteAsync();
                await ctx.Telegrams.Where(t => t.NotifierId == id || t.RecipientId == id).ExecuteDeleteAsync();
                await ctx.Subscriptions.Where(s => s.UserId == id).ExecuteDeleteAsync();
                await ctx.Penalties.Where(p => p.PenalizedId == id).ExecuteDeleteAsync();
                await ctx.GuestClearances.Where(c => c.UserId == id).ExecuteDeleteAsync();
                await ctx.UserRelationships.Where(l => l.SelfId == id).ExecuteDeleteAsync();
                await ctx.GatheringLinks.Where(l => l.UserId == id).ExecuteDeleteAsync();
                await ctx.ProfileMessages.Where(m => m.ProfileId == id).ExecuteUpdateAsync(setter => setter.SetProperty(r => r.ProfileId, (long?)null));
                await ctx.Messages.Where(m => m.UserId == id).ExecuteUpdateAsync(setter => setter.SetProperty(r => r.UserId, (long?)null));
                await ctx.Feedback.Where(r => r.UserId == id).ExecuteUpdateAsync(setter => setter.SetProperty(r => r.UserId, (long?)null));
                await ctx.UserReports.Where(r => r.SelfId == id).ExecuteUpdateAsync(setter => setter.SetProperty(r => r.SelfId, (long?)null));
                await ctx.GatheringReports.Where(r => r.UserId == id).ExecuteUpdateAsync(setter => setter.SetProperty(r => r.UserId, (long?)null));
                await ctx.SnapshotReports.Where(r => r.UserId == id).ExecuteUpdateAsync(setter => setter.SetProperty(r => r.UserId, (long?)null));
                await ctx.Gatherings.Where(g => g.HostId == id).ExecuteUpdateAsync(setter => setter.SetProperty(g => g.HostId, (long?)null));
                await ctx.Users.Where(u => u.Id == id).ExecuteDeleteAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<CoreUser> FindUserByIdAsync(long id)
        {
            await using CanaryContext ctx = initContext();

            return await ctx.Users.
              Where(u => u.Id == id).
              Select(u => new CoreUser
              (
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
              )).SingleAsync();
        }
        public async Task<CoreUser> FindUserByPhoneNumberAsync(string phoneNumber)
        {
            await using CanaryContext ctx = initContext();

            return await ctx.Users.
                 Where(u => u.PhoneNumber == phoneNumber).
                 Select(u => new CoreUser
                 (
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
                 )).SingleAsync();
        }
        public async Task<CoreUser> FindUserByEmailAsync(string email)
        {
            await using CanaryContext ctx = initContext();

            return await ctx.Users.
              Where(u => u.Email == email).
              Select(u => new CoreUser
              (
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
              )).SingleAsync();
        }

        public async Task<HauntShard> GetUserHauntAsync(long id)
        {
            await using CanaryContext ctx = initContext();

            return await 
                ctx.Users.
                Where(u => u.Id == id).
                Select(u => new HauntShard(u.Haunt.Y, u.Haunt.X, u.HauntRadius, u.HauntWeight)).
                SingleAsync();
        }
        public async Task<LocationShard> GetRecentLocationAsync(long id)
        {
            await using CanaryContext ctx = initContext();

            return await 
                ctx.Users.
                Where(u => u.Id == id).
                Select(u => new LocationShard(u.CurrentLocation.Y, u.CurrentLocation.X, u.CurrentRadius)).
                SingleAsync();
        }

        public async Task UpdateUserAsync(long id, List<(string Property, object Value)> edits)
        {
            await using CanaryContext ctx = initContext();

            User u = new() { Id = id };

            ctx.Users.Attach(u);

            foreach ((string Property, object Value) in edits)
            {
                switch (Property)
                {
                    case "PhoneNumber":
                        u.PhoneNumber = (string)Value;
                        break;
                    case "Email":
                        u.Email = (string)Value;
                        break;
                    case "NormalisedEmail":
                        u.NormalisedEmail = (string)Value;
                        break;
                    case "Name":
                        u.Name = (string)Value;
                        break;
                    case "IsPhoneConfirmed":
                        u.IsPhoneConfirmed = (bool)Value;
                        break;
                    case "IsEmailConfirmed":
                        u.IsEmailConfirmed = (bool)Value;
                        break;
                    case "SecurityStamp":
                        u.SecurityStamp = (string)Value;
                        break;
                    case "LockoutDate":
                        u.LockoutDate = (DateTimeOffset?)Value;
                        break;
                    case "AccessTries":
                        u.AccessTries = (int)Value;
                        break;
                    case "AccountStatus":
                        u.AccountStatus = (UserAccountStatus)Value;
                        break;
                    case "Reputation":
                        u.Reputation = (int)Value;
                        break;
                    default:
                        throw new InvalidInputException("Property named \"" + Property + "\" can not be updated using this method.");
                }
                ctx.Entry(u).Property(Property).IsModified = true;
            }
            await ctx.SaveChangesAsync();
        }

        public async Task UpdateHauntAsync(long id, double latitude, double longitude, double radius, int stability)
        {
            await using CanaryContext ctx = initContext();

            Point newHaunt = new CoordinateFactory().Create(longitude, latitude);
            User u = new() { Id = id, Haunt = newHaunt, HauntRadius = radius, HauntWeight = stability };

            ctx.Users.Attach(u);
            ctx.Entry(u).Property(nameof(u.Haunt)).IsModified = true;
            ctx.Entry(u).Property(nameof(u.HauntRadius)).IsModified = true;
            ctx.Entry(u).Property(nameof(u.HauntWeight)).IsModified = true;
            await ctx.SaveChangesAsync();
        }

        public async Task UpdateRecentLocationAsync(long id, double latitude, double longitude, double radius)
        {
            await using CanaryContext ctx = initContext();

            Point newCurrentLocation = new CoordinateFactory().Create(longitude, latitude);
            User u = new() { Id = id, CurrentLocation = newCurrentLocation, CurrentRadius = radius };

            ctx.Users.Attach(u);
            ctx.Entry(u).Property(nameof(u.CurrentLocation)).IsModified = true;
            ctx.Entry(u).Property(nameof(u.CurrentRadius)).IsModified = true;
            await ctx.SaveChangesAsync();
        }

        public async Task<bool> UserExistsAsync(string phoneNumber)
        {
            await using CanaryContext ctx = initContext();
            return await ctx.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task<string> RerollUserCodeAsync(long userId)
        {
            await using CanaryContext ctx = initContext();

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

                codeUnique = !(await ctx.Users.AnyAsync(u => u.CompanionshipCode == potentialCode));
            }

            User u = new() { Id = userId, CompanionshipCode = potentialCode };


            ctx.Users.Attach(u);
            ctx.Entry(u).Property(nameof(u.CompanionshipCode)).IsModified = true;
            await ctx.SaveChangesAsync();

            return potentialCode;
        }

        public async Task<CoreUser> FindUserByCodeAsync(string code)
        {
            await using CanaryContext ctx = initContext();

            return await ctx.Users.
                Where(u => u.CompanionshipCode == code).
                Select(u => new CoreUser
                (
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
                )).SingleAsync();
        }
    }
}
