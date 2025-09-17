using Core.Boundaries;
using CrazyLizard.Exceptions;
using CrazyLizard.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OneSignalApi.Model;
using Repository.Entities;
using Stripe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static FastEndpoints.Ep;
using Subscription = Stripe.Subscription;

namespace CrazyLizard.Services
{
    public class AccountService(IAccountRepository accountRepository, ICircleRepository circleRepository, StripeClient stripeClient) : IAccountService
    {
        public async Task<bool> UserExistsAsync(string phoneNumber)
        {
            return await accountRepository.PhoneNumberExistsAsync(phoneNumber);
        }

		public async Task<CoreUser> GetCoreUserAsync(long userId)
        {
            return await accountRepository.GetUserByIdAsync(userId);
        }

        public async Task<CoreUser> GetCoreUserAsync(string phoneNumber)
		{
            return await accountRepository.GetUserByPhoneNumberAsync(phoneNumber);
        }

        public async Task CreateUserAsync(string phoneNumber, string email, string title, string givenName, string familyName, DateOnly dateOfBirth)
        {
            await accountRepository.CreateUserAsync(phoneNumber, email, email,
                title, givenName, familyName, dateOfBirth, DateTimeOffset.UtcNow,
                Guid.NewGuid());
        }

        public async Task EditUserAsync(long userId,
            string phoneNumber = null, string email = null,
            string title = null, string givenName = null, string familyName = null,
			DateOnly? dateOfBirth = null, bool? isPhoneNumberConfirmed = null, bool? isEmailConfirmed = null,
			string securityStamp = null, DateTimeOffset? lockoutDate = null, int? accessTries = null)
        {
            // Throws if user not found or locked
            CoreUser user = await accountRepository.GetUserByIdAsync(userId);
            
            // Check unique details changed to avoid errors
            bool phoneNumberChanged = !string.IsNullOrEmpty(phoneNumber) && user.PhoneNumber != phoneNumber;
            bool emailChanged = !string.IsNullOrEmpty(email) && user.Email != email;
            bool dateOfBirthChanged = dateOfBirth.HasValue;
            bool titleChanged = !string.IsNullOrEmpty(title);
            bool givenNameChanged = !string.IsNullOrEmpty(givenName);
            bool familyNameChanged = !string.IsNullOrEmpty(familyName);

            // Modify user for validation
            string newPhoneNumber = phoneNumberChanged ? phoneNumber : user.PhoneNumber;
            string newEmail = emailChanged ? email : user.Email;
            string newTitle = titleChanged ? title : user.Title;
            string newGivenName = givenNameChanged ? givenName : user.FirstName;
            string newFamilyName = familyNameChanged ? familyName : user.LastName;
            DateOnly newDateOfBirth = dateOfBirthChanged ? dateOfBirth.Value : user.DateOfBirth;

            List<(string Property, object Value)> edits = new();
            // Gather individual edits
			if (phoneNumberChanged)
            {
                edits.Add((nameof(CoreUser.PhoneNumber), user.PhoneNumber));
            }
			if (emailChanged)
			{

                edits.Add((nameof(CoreUser.Email), email));
                edits.Add(("NormalisedEmail", user.Email));
                edits.Add((nameof(CoreUser.IsEmailConfirmed), false));
            }
			if (titleChanged)
			{
                edits.Add((nameof(CoreUser.Title), user.Title));
			}
			if (givenNameChanged)
			{
                edits.Add((nameof(CoreUser.FirstName), user.FirstName));
			}
			if (familyNameChanged)
			{
                edits.Add((nameof(CoreUser.LastName), user.LastName));
			}
			if (dateOfBirthChanged)
			{
                edits.Add((nameof(CoreUser.DateOfBirth), user.DateOfBirth));
			}
            // Internal attributes for account store
			if (isPhoneNumberConfirmed != null)
			{
                edits.Add((nameof(CoreUser.IsPhoneConfirmed), isPhoneNumberConfirmed.Value));
			}
			if (isEmailConfirmed != null)
			{
                edits.Add((nameof(CoreUser.IsEmailConfirmed), isEmailConfirmed.Value));
			}
			if (!string.IsNullOrEmpty(securityStamp))
			{
                edits.Add((nameof(CoreUser.SecurityStamp), securityStamp));
			}
			if (lockoutDate != null)
			{
                edits.Add((nameof(CoreUser.LockoutDate), lockoutDate.Value));
			}
			if (accessTries != null)
			{
                edits.Add((nameof(CoreUser.AccessTries), accessTries.Value));
			}

            // Push update
            await accountRepository.UpdateUserAsync(user.Id, edits);
		}

        public async Task UpdateUserAgreementAsync(long userId)
        {
            var user = await accountRepository.GetUserByIdAsync(userId);

            await accountRepository.UpdateUserAsync(user.Id,
                new() { (nameof(CoreUser.TimeOfUserAgreement), DateTimeOffset.UtcNow) });
        }

        public async Task EditAvatarAsync(long userId, MemoryStream image)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteUserAsync(long userId)
        {
            await accountRepository.DeleteUserAsync(userId);
        }

        public async Task<string> CreateSetupIntent(long userId, CancellationToken cancellationToken = default)
        {
            CoreUser user = await accountRepository.GetUserByIdAsync(userId);

            Customer customer;
            if (!string.IsNullOrEmpty(user.StripeCustomerId))
            {
                customer = await stripeClient.V1.Customers.GetAsync(user.StripeCustomerId, cancellationToken: cancellationToken);
            }
            else
            {
                CustomerCreateOptions customerOptions = new()
                {
                    Name = $"{user.Title} {user.FirstName} {user.LastName}",
                    Phone = user.PhoneNumber,
                };

                customer = await stripeClient.V1.Customers.CreateAsync(customerOptions, cancellationToken: cancellationToken);
                await accountRepository.UpdateStripeCustomerIdAsync(user.Id, customer.Id);
            }

            SetupIntentCreateOptions options = new()
            {
                Customer = customer.Id,
            };

            SetupIntent setupIntent = await stripeClient.V1.SetupIntents.CreateAsync(options, cancellationToken: cancellationToken);

            return setupIntent.ClientSecret;
        }

        public async Task UpdateStripeCustomerIdAsync(long userId, string newId)
        {
            await accountRepository.UpdateStripeCustomerIdAsync(userId, newId);

        }
        public async Task UpdateStripeSubscriptionId(long userId, string newId)
        {
            await accountRepository.UpdateStripeSubscriptionId(userId, newId);
        }

        public async Task EditRecipientAsync(long userId, long recipientId, List<(string Property, object Value)> edits)
        {
            if (!await accountRepository.IsManagerAsync(userId, recipientId))
                throw new NoAccessException($"User {userId} does not manage recipient {recipientId}.");

            await accountRepository.UpdateRecipientAsync(recipientId, edits);
        }

        public async Task RemoveRecipientAsync(long userId, long recipientId, CancellationToken cancellationToken = default)
        {
            if (!await circleRepository.HasCircle(userId))
                throw new NotFoundException($"User {userId} does not have a circle.");

            if (!await accountRepository.IsManagerAsync(userId, recipientId))
                throw new NoAccessException($"User {userId} does not manage recipient {recipientId}.");

            CoreUser user = await accountRepository.GetUserByIdAsync(userId);

            if (!user.ProvidedPaymentDetails)
                throw new NotFoundException($"User {userId} has not provided payment details.");

            if (string.IsNullOrWhiteSpace(user.StripeSubscriptionId))
                throw new NotFoundException($"User {userId} does not have a subscription.");

            Subscription subscription = await stripeClient.V1.Subscriptions.GetAsync(user.StripeSubscriptionId, cancellationToken: cancellationToken);

            if (subscription.Items.Count() > 1)
            {
                SubscriptionItem subscriptionItem = subscription.Items.First();

                SubscriptionItemUpdateOptions subscriptionItemOptions = new()
                {
                    Quantity = subscriptionItem.Quantity - 1,
                    ProrationBehavior = "none",
                };

                await stripeClient.V1.SubscriptionItems.UpdateAsync(subscriptionItem.Id, subscriptionItemOptions, cancellationToken: cancellationToken);
            }
            else
            {
                SubscriptionCancelOptions subscriptionOptions = new()
                {
                    InvoiceNow = false,
                    Prorate = false
                };

                await stripeClient.V1.Subscriptions.CancelAsync(subscription.Id, subscriptionOptions, cancellationToken: cancellationToken);
            }

            await accountRepository.RemoveRecipientAsync(recipientId);
        }

        public async Task AddRecipientAsync(long userId, CoreRecipient recipient, CancellationToken cancellationToken = default)
        {
            if (!await circleRepository.HasCircle(userId))
                throw new NotFoundException($"User {userId} does not have a circle.");

            CoreUser user = await accountRepository.GetUserByIdAsync(userId);

            if (!user.ProvidedPaymentDetails)
                throw new NotFoundException($"User {userId} has not provided payment details.");
         
            if (!string.IsNullOrWhiteSpace(user.StripeSubscriptionId))
            {
                Subscription subscription = await stripeClient.V1.Subscriptions.GetAsync(user.StripeSubscriptionId, cancellationToken: cancellationToken);

                SubscriptionItem subscriptionItem = subscription.Items.First();

                SubscriptionItemUpdateOptions subscriptionItemOptions = new()
                {
                    Quantity = subscriptionItem.Quantity + 1,
                    ProrationBehavior = "none",
                };

                await stripeClient.V1.SubscriptionItems.UpdateAsync(subscriptionItem.Id, subscriptionItemOptions, cancellationToken: cancellationToken);
            }
            else
            {
                SubscriptionCreateOptions subscriptionOptions = new()
                {
                    Customer = user.StripeCustomerId,
                    Items =
                    [
                        new()
                        {
                            Price = "prod_T3oReCcNZqq7wm",
                            Quantity = 1,
                        },
                    ],
                    PaymentSettings =  { SaveDefaultPaymentMethod = "on_subscription" },
                    PaymentBehavior = "default_incomplete",
                    BillingMode = new() { Type = "flexible" },
                };

                await stripeClient.V1.Subscriptions.CreateAsync(subscriptionOptions, cancellationToken: cancellationToken);
            }

            await accountRepository.AddRecipientAsync(recipient);
        }
    }
}
