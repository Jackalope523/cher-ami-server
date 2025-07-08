using Microsoft.EntityFrameworkCore;
using Repository.Databases.Contexts;
using Repository.Databases.Entities;

namespace Repository.Databases.Stores
{
    public class AccountRepository : Repository, IAccountDatabase
    {
        internal AccountRepository(Func<CanaryContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task<CoreUser> CreateUserAsync(string phoneNumber, string email, string normalisedEmail, string title, string givenName, string familyName, DateTimeOffset dateOfBirth, DateTimeOffset joinDate, Guid notificationId)
        {
            User toCreate = new()
            {
                PhoneNumber = phoneNumber,
                Email = email,
                NormalizedEmail = normalisedEmail,
                Title = title,
                FirstName = givenName,
                LastName = familyName,
                DateOfBirth = dateOfBirth,
                JoinDate = joinDate,
                NotificationId = notificationId,
            };

            await using (CanaryContext ctx = initContext())
            {
                ctx.Users.Add(toCreate);
                await ctx.SaveChangesAsync();
            }

            return new CoreUser
              (
                  toCreate.Id,
                  toCreate.PhoneNumber,
                  toCreate.Email,
                  toCreate.NormalizedEmail,
                  toCreate.Title,
                  toCreate.FirstName,
                  toCreate.LastName,
                  toCreate.DateOfBirth,
                  toCreate.IsPhoneConfirmed,
                  toCreate.IsEmailConfirmed,
                  toCreate.SoftDeleted,
                  toCreate.SecurityStamp,
                  toCreate.LockoutDate,
                  toCreate.AccessTries,
                  toCreate.AccountStatus,
                  toCreate.JoinDate,
                  toCreate.TimeOfUserAgreement,
                  toCreate.NotificationId
              );
        }

        public async Task SoftDeleteAsync(long id)
        {
            await using CanaryContext ctx = initContext();
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
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

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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

        public async Task<CoreUser> GetUserByIdAsync(long id) 
        {
            await using CanaryContext ctx = initContext();

            return await ctx.Users.
              Where(u => u.Id == id).
              Select(u => new CoreUser
              (
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
              )).SingleAsync();
        }

        public async Task<CoreUser> GetUserByPhoneNumberAsync(string phoneNumber) 
        {
            await using CanaryContext ctx = initContext();

            return await ctx.Users.
                 Where(u => u.PhoneNumber == phoneNumber).
                 Select(u => new CoreUser
                 (
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
                 )).SingleAsync();
        }

        public async Task<CoreUser> GetUserByEmailAsync(string email) 
        {
            await using CanaryContext ctx = initContext();

            return await ctx.Users.
              Where(u => u.Email == email).
              Select(u => new CoreUser
              (
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
              )).SingleAsync();
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
                        u.NormalizedEmail = (string)Value;
                        break;
                    case "Title":
                        u.Title = (string)Value;
                        break;
                    case "GivenName":
                        u.FirstName = (string)Value;
                        break;
                    case "FamilyName":
                        u.LastName = (string)Value;
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

        public async Task<bool> PhoneNumberExistsAsync(string phoneNumber)
        {
            await using CanaryContext ctx = initContext();
            return await ctx.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task<bool> EmailExistsAsync(string normalisedEmail)
        {
            await using CanaryContext ctx = initContext();
            return await ctx.Users.AnyAsync(u => u.NormalizedEmail == normalisedEmail);
        }
    }
}
