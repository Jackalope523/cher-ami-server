using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
using Serilog;

namespace Repository
{
    public class EFCoreAccountStore : QueryStore, IAccountDatabase
    {
        public EFCoreAccountStore(Harbor.Flag flag) : base(flag)
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

            await storeSentry.ExecuteWriteAsync(ctx => ctx.Users.Add(toCreate));
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
            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.ChatLinks.
               Where(l => l.UserId == id).
               ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.Connections.
               Where(c => c.UserId == id).
               ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Notifications.
                Where(n => n.RecipientId == id).
                ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.SnapshotLinks.
                Where(s => s.UserId == id).
                ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Snapshots.
                Where(s => s.OwnerId == id).
                ExecuteUpdate(setter => setter.SetProperty(s => s.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Telegrams.
                Where(t => t.NotifierId == id || t.RecipientId == id).
                ExecuteUpdate(setter => setter.SetProperty(s => s.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Subscriptions.
                Where(s => s.UserId == id).
                ExecuteUpdate(setter => setter.SetProperty(s => s.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Penalties.
                Where(p => p.PenalizedId == id).
                ExecuteUpdate(setter => setter.SetProperty(s => s.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.GuestClearances.
                Where(c => c.UserId == id).
                ExecuteUpdate(setter => setter.SetProperty(s => s.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.UserRelationships.
               Where(l => l.SelfId == id || l.OtherId == id).
               ExecuteUpdate(setter => setter.SetProperty(s => s.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.GatheringLinks.
               Where(l => l.UserId == id).
               ExecuteUpdate(setter => setter.SetProperty(s => s.SoftDeleted, true)));

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.Users.
               Where(u => u.Id == id).
               ExecuteUpdate(setter => setter.SetProperty(s => s.SoftDeleted, true)));
        }

        public async Task HardDeleteAsync(long id)
        {
            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Connections.
                Where(c => c.UserId == id).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.ChatLinks.
               Where(l => l.UserId == id).
               ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Notifications.
                Where(n => n.RecipientId == id).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.SnapshotLinks.
                Where(s => s.UserId == id).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Snapshots.
                Where(s => s.OwnerId == id).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Telegrams.
                Where(t => t.NotifierId == id || t.RecipientId == id).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.Subscriptions.
               Where(s => s.UserId == id).
               ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.Penalties.
               Where(p => p.PenalizedId == id).
               ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.GuestClearances.
               Where(c => c.UserId == id).
               ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.UserRelationships.
               Where(l => l.SelfId == id).
               ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.GatheringLinks.
               Where(l => l.UserId == id).
               ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.ProfileMessages.
               Where(m => m.ProfileId == id).
               ExecuteUpdate(setter => setter.SetProperty(r => r.ProfileId, (long?)null)));

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.Messages.
               Where(m => m.UserId == id).
               ExecuteUpdate(setter => setter.SetProperty(r => r.UserId, (long?)null)));

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.Feedback.
               Where(r => r.UserId == id).
               ExecuteUpdate(setter => setter.SetProperty(r => r.UserId, (long?)null)));

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.UserReports.
               Where(r => r.SelfId == id).
               ExecuteUpdate(setter => setter.SetProperty(r => r.SelfId, (long?)null)));

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.GatheringReports.
               Where(r => r.UserId == id).
               ExecuteUpdate(setter => setter.SetProperty(r => r.UserId, (long?)null)));

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.SnapshotReports.
               Where(r => r.UserId == id).
               ExecuteUpdate(setter => setter.SetProperty(r => r.UserId, (long?)null)));

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.Gatherings.
               Where(g => g.HostId == id).
               ExecuteUpdate(setter => setter.SetProperty(g => g.HostId, (long?)null)));        

            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.Users.
               Where(u => u.Id == id).
               ExecuteDeleteAsync());
        }

        public async Task<CoreUser> FindUserByIdAsync(long id) 
        {
            CoreUser user;
            try 
            {
               user = await storeSentry.ExecuteReadAsync(ctx => 
               ctx.Users.
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
               )).SingleAsync());
            }
            catch (InvalidOperationException ex)
            {
                throw new UserNotFoundException("Unable to find a user bearing supplied Id. It has possibly been soft deleted.", ex);
            }

            return user;
        }
        public async Task<CoreUser> FindUserByPhoneNumberAsync(string phoneNumber) 
        {
            CoreUser user;
            try
            {
              user = await storeSentry.ExecuteReadAsync(ctx => 
              ctx.Users.
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
              )).SingleAsync());
            }
            catch (InvalidOperationException ex)
            {
                throw new UserNotFoundException("Unable to find a user bearing supplied Id.", ex);
            }

            return user;
        }
        public async Task<CoreUser> FindUserByEmailAsync(string email) 
        { 
            CoreUser user;
            try
            {
              user = await storeSentry.ExecuteReadAsync(ctx => 
              ctx.Users.
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
              )).SingleAsync());
            }
            catch (InvalidOperationException ex)
            {
                throw new UserNotFoundException("Unable to find a user bearing supplied Id.", ex);
            }

            return user;
        }

        public async Task<HauntShard> GetUserHauntAsync(long id)
        {
            try
            {
                return await storeSentry.ExecuteReadAsync(ctx => 
                ctx.Users.
                Where(u => u.Id == id).
                Select(u => new HauntShard(u.Haunt.Y, u.Haunt.X, u.HauntRadius, u.HauntWeight)).
                SingleAsync());
            }
            catch (InvalidOperationException ex)
            {
                throw new UserNotFoundException("Unable to find a user bearing supplied Id.", ex);
            }
        }
        public async Task<LocationShard> GetRecentLocationAsync(long id)
        {       
            try
            {
                return await storeSentry.ExecuteReadAsync(ctx =>
                            ctx.Users.
                            Where(u => u.Id == id).
                            Select(u => new LocationShard(u.CurrentLocation.Y, u.CurrentLocation.X, u.CurrentRadius)).
                            SingleAsync());

            }
            catch (InvalidOperationException ex)
            {
                throw new UserNotFoundException("Unable to find a user bearing supplied Id.", ex);
            }                 
        }    

        public async Task UpdateUserAsync(long id, List<(string Property, object Value)> edits)
        {
            Discussion currentDiscussion = storeSentry.BeginDiscussion();

            User u = new() { Id = id };

            storeSentry.DiscussWrite(ctx => ctx.Users.Attach(u), currentDiscussion);

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
                storeSentry.DiscussWrite(ctx => ctx.Entry(u).Property(Property).IsModified = true, currentDiscussion);
            }
            await storeSentry.EndDiscussionAsync(currentDiscussion);
        }

        public async Task UpdateHauntAsync(long id, double latitude, double longitude, double radius, int stability)
        {
            Discussion currentDiscussion = storeSentry.BeginDiscussion();

            Point newHaunt = new CoordinateFactory().Create(longitude, latitude);
            User u = new() { Id = id, Haunt = newHaunt , HauntRadius = radius, HauntWeight = stability };

            storeSentry.DiscussWrite(ctx => ctx.Users.Attach(u), currentDiscussion);
            storeSentry.DiscussWrite(ctx => ctx.Entry(u).Property(nameof(u.Haunt)).IsModified = true, currentDiscussion);
            storeSentry.DiscussWrite(ctx => ctx.Entry(u).Property(nameof(u.HauntRadius)).IsModified = true, currentDiscussion);
            storeSentry.DiscussWrite(ctx => ctx.Entry(u).Property(nameof(u.HauntWeight)).IsModified = true, currentDiscussion);
            await storeSentry.EndDiscussionAsync(currentDiscussion);
        }

        public async Task UpdateRecentLocationAsync(long id, double latitude, double longitude, double radius)
        {
            Discussion currentDiscussion = storeSentry.BeginDiscussion();

            Point newCurrentLocation = new CoordinateFactory().Create(longitude, latitude);
            User u = new() { Id = id, CurrentLocation = newCurrentLocation, CurrentRadius = radius };

            storeSentry.DiscussWrite(ctx => ctx.Users.Attach(u), currentDiscussion);
            storeSentry.DiscussWrite(ctx => ctx.Entry(u).Property(nameof(u.CurrentLocation)).IsModified = true, currentDiscussion);
            storeSentry.DiscussWrite(ctx => ctx.Entry(u).Property(nameof(u.CurrentRadius)).IsModified = true, currentDiscussion);
            await storeSentry.EndDiscussionAsync(currentDiscussion);
        }

        public async Task<bool> UserExistsAsync(string phoneNumber)
        {
            return await storeSentry.ExecuteReadAsync(ctx => ctx.Users.AnyAsync(u => u.PhoneNumber == phoneNumber));
        }

        public async Task<string> RerollUserCodeAsync(long userId)
        {
            List<string> adjectives = await storeSentry.ExecuteReadAsync(ctx => 
                                        ctx.Words.
                                        Where(w => w.Type == Word.WordType.Adjective).
                                        Select(w => w.Text).
                                        ToListAsync());

            List<string> nouns = await storeSentry.ExecuteReadAsync(ctx =>
                                        ctx.Words.
                                        Where(w => w.Type == Word.WordType.Noun).
                                        Select(w => w.Text).
                                        ToListAsync());

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

                codeUnique = !(await storeSentry.ExecuteReadAsync(ctx => ctx.Users.AnyAsync(u => u.CompanionshipCode == potentialCode)));
            }

            User u = new() { Id = userId, CompanionshipCode = potentialCode };

            Discussion currentDiscussion = storeSentry.BeginDiscussion();
            storeSentry.DiscussWrite(ctx => ctx.Users.Attach(u), currentDiscussion);
            storeSentry.DiscussWrite(ctx => ctx.Entry(u).Property(nameof(u.CompanionshipCode)).IsModified = true, currentDiscussion);
            await storeSentry.EndDiscussionAsync(currentDiscussion);

            return potentialCode;
        }

        public async Task<CoreUser> FindUserByCodeAsync(string code)
        {
            return await storeSentry.ExecuteReadAsync(ctx => 
                ctx.Users.
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
                )).SingleAsync());
        }
    }
}
