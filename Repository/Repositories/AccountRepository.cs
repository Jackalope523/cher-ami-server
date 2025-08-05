using Microsoft.EntityFrameworkCore;
using Repository.Contexts;
using Repository.Entities;

namespace Repository.Repositories
{
    public class AccountRepository : Repository, IAccountRepository
    {
        internal AccountRepository(Func<CardinalContext> contextFactory) : base(contextFactory)
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

            await using (CardinalContext ctx = initContext())
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

        public async Task DeleteUserAsync(long id)
        {

            // [ ] Chats
            // [ ] Messages
            // [ ] Reports
            // [ ] Captions
            // [ ] ChatMemberships
            // [ ] Circles
            // [ ] CircleMemberships
            // [ ] Connections
            // [ ] Feedbacks
            // [ ] Issues
            // [ ] Notifications
            // [ ] Posts
            // [ ] Recipients
            // [ ] Snapshots
            // [ ] Subscriptions
            // [ ] Users
            // [ ] Words
            // [ ] CircleRecipients

            await using CardinalContext ctx = initContext();
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                await ctx.Notifications.Where(n => n.RecipientId == id).ExecuteDeleteAsync();
                await ctx.Subscriptions.Where(s => s.UserId == id).ExecuteDeleteAsync();
                await ctx.CircleMemberships.Where(l => l.UserId == id).ExecuteDeleteAsync();
                await ctx.Feedback.Where(r => r.UserId == id).ExecuteUpdateAsync(setter => setter.SetProperty(r => r.UserId, (long?)null));
                await ctx.Reports.Where(r => r.FilingUserId == id).ExecuteUpdateAsync(setter => setter.SetProperty(r => r.FilingUserId, (long?)null));
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
            await using CardinalContext ctx = initContext();

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
            await using CardinalContext ctx = initContext();

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
            await using CardinalContext ctx = initContext();

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
            await using CardinalContext ctx = initContext();

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
            await using CardinalContext ctx = initContext();
            return await ctx.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task<bool> EmailExistsAsync(string normalisedEmail)
        {
            await using CardinalContext ctx = initContext();
            return await ctx.Users.AnyAsync(u => u.NormalizedEmail == normalisedEmail);
        }
    }
}
