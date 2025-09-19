using Core.Boundaries;
using CrazyLizard.Contexts;
using CrazyLizard.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CrazyLizard.Repositories
{
    public class AccountRepository(DatabaseContext ctx) : IAccountRepository
    {

        public async Task<User> CreateUserAsync(string phoneNumber, string email, string normalisedEmail, string title, string givenName, string familyName, DateOnly dateOfBirth, DateTimeOffset joinDate, Guid notificationId)
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

            ctx.Users.Add(toCreate);
            await ctx.SaveChangesAsync();

            return toCreate;
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

            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                await ctx.Notifications.Where(n => n.RecipientId == id).ExecuteDeleteAsync();
                await ctx.Subscriptions.Where(s => s.UserId == id).ExecuteDeleteAsync();
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

        public async Task<User> GetUserByIdAsync(long id) 
        {
            return await ctx.Users.Where(u => u.Id == id).SingleAsync();
        }

        public async Task<User> GetUserByPhoneNumberAsync(string phoneNumber) 
        {
            return await ctx.Users.Where(u => u.PhoneNumber == phoneNumber).SingleOrDefaultAsync();
        }

        public async Task<User> GetUserByEmailAsync(string email) 
        {
            return await ctx.Users.Where(u => u.Email == email).SingleAsync();
        }

        public async Task UpdateUserAsync(long id, List<(string Property, object Value)> edits)
        {
            User u = await ctx.Users.FindAsync(id);

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
                    default:
                        throw new ArgumentException("Property named \"" + Property + "\" can not be updated using this method.");
                }
            }

            await ctx.SaveChangesAsync();
        }

        public async Task<bool> PhoneNumberExistsAsync(string phoneNumber)
        {
            return await ctx.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task<bool> EmailExistsAsync(string normalisedEmail)
        {
            return await ctx.Users.AnyAsync(u => u.NormalizedEmail == normalisedEmail);
        }

        public async Task<bool> ShareCircle(long userId1, long userId2)
        {
            long? userOneCircle = await ctx.Users.Where(x => x.Id == userId1).Select(x => x.CircleId).SingleOrDefaultAsync();
            long? userTwoCircle = await ctx.Users.Where(x => x.Id == userId2).Select(x => x.CircleId).SingleOrDefaultAsync();

            if (userOneCircle == null || userTwoCircle == null) return false;
            
            return userOneCircle == userTwoCircle;
        }

        public async Task UpdateStripeCustomerIdAsync(long userId, string newId)
        {
            User user = await ctx.Users.FindAsync(userId);
            user.StripeCustomerId = newId;
            await ctx.SaveChangesAsync();

        }

        public async Task UpdateStripeSubscriptionIdAsync(long userId, string newId)
        {
            User user = await ctx.Users.FindAsync(userId);
            user.StripeSubscriptionId = newId;
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
                    case nameof(CoreRecipient.Address.UnitNumber):
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

        public async Task RemoveRecipientAsync(long recipientId)
        {
            await ctx.Recipients.Where(x => x.Id == recipientId).ExecuteDeleteAsync();
        }

        public async Task AddRecipientAsync(CoreRecipient recipient)
        {
            Recipient toAdd = new()
            {
                Title = recipient.Title,
                FirstName = recipient.FirstName,
                LastName = recipient.LastName,
                StreetAddress = recipient.Address.Street,
                UnitNumber = recipient.Address.UnitNumber,
                City = recipient.Address.City,
                ProvinceOrState = recipient.Address.ProvinceOrState,
                PostalCode = recipient.Address.PostalCode,
                Country = recipient.Address.Country,
                ManagerId = recipient.ManagerId,
                State = RecipientState.Inactive,
            };

            ctx.Recipients.Add(toAdd);
            await ctx.SaveChangesAsync();
        }

        public async Task<bool> IsManagerAsync(long userId, long recipientId)
        {
            return await ctx.Recipients.AnyAsync(x => x.Id == recipientId && x.ManagerId == userId);
        }

        public async Task ConfirmPaymentDetailsProvidedAsync(string stripeCustomerId)
        {
            await ctx.Users
                .Where(x => x.StripeCustomerId == stripeCustomerId)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(x => x.ProvidedPaymentDetails, true)
                );
        }
    }
}
