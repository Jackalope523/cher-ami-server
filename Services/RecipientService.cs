using CherAmiAPI.Entities;
using CherAmiAPI.Exceptions;
using CherAmiAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Stripe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Services
{
    public class RecipientService(
        IRecipientRepository recipientRepository,
        IUserRepository userRepository,
        IImageService imageService,
        IUnitOfWork unitOfWork,
        CustomerPaymentMethodService customerPaymentMethodService)
    {
        public async Task<Recipient> AddRecipientAsync(long managerId, Recipient toAdd, IFormFile avatar = null, CancellationToken cancellationToken = default)
        {
            User manager = await userRepository.GetUserAsync(managerId, cancellationToken);

            if (!manager.IsBillingExempt && (await customerPaymentMethodService.ListAsync(manager.StripeCustomerId, cancellationToken: cancellationToken)).Data.Count == 0)
                throw new NoPermissionException($"User {managerId} has not provided a payment method.");

            toAdd.ManagerId = managerId;

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // Save first so the avatar path is built from the database-assigned id
                await recipientRepository.AddRecipientAsync(toAdd, cancellationToken);

                if (avatar != null)
                {
                    using MemoryStream stream = new();
                    await avatar.CopyToAsync(stream, cancellationToken);

                    string path = $"users/{managerId}/recipients/{toAdd.Id}/avatar.jpg";
                    toAdd.AvatarPath = path;
                    toAdd.AvatarTimestamp = DateTimeOffset.UtcNow;

                    await recipientRepository.SaveRecipientAsync(toAdd, cancellationToken);

                    await imageService.UploadImageAsync(path, stream);
                }
            }, cancellationToken);

            return toAdd;
        }

        public async Task<Recipient> GetRecipientAsync(long requesterId, long recipientId, CancellationToken cancellationToken = default)
        {
            Recipient recipient = await recipientRepository.GetRecipientAsync(recipientId, cancellationToken);

            if (requesterId != recipient.ManagerId && !await userRepository.ShareCommonCircleAsync(cancellationToken, requesterId, recipient.ManagerId))
                throw new NoAccessException($"User {requesterId} can not access this recipient.");

            return recipient;
        }

        public async Task UpdateRecipientAsync(
            long managerId,
            long recipientId,
            string title,
            string name,
            string addressLine1,
            string addressLine2,
            string city,
            string provinceOrState,
            string postalCode,
            string country,
            bool? isVeteran,
            IFormFile avatar = null,
            CancellationToken cancellationToken = default)
        {
            Recipient recipient = await recipientRepository.GetRecipientAsync(recipientId, cancellationToken);

            if (recipient.ManagerId != managerId)
                throw new NoAccessException();

            recipient.Title = title;
            recipient.Name = name;
            recipient.AddressLine1 = addressLine1;
            recipient.AddressLine2 = addressLine2;
            recipient.City = city;
            recipient.ProvinceOrState = provinceOrState;
            recipient.PostalCode = postalCode;
            recipient.Country = country;
            recipient.IsVeteran = isVeteran ?? recipient.IsVeteran;

            if (avatar != null)
            {
                using MemoryStream stream = new();
                await avatar.CopyToAsync(stream, cancellationToken);

                string path = $"users/{managerId}/recipients/{recipientId}/avatar.jpg";

                recipient.AvatarPath = path;
                recipient.AvatarTimestamp = DateTimeOffset.UtcNow;

                await imageService.UploadImageAsync(path, stream);
            }

            await recipientRepository.SaveRecipientAsync(recipient, cancellationToken);
        }

        public async Task DeleteRecipientAsync(long managerId, long recipientId, CancellationToken cancellationToken = default)
        {
            Recipient recipient = await recipientRepository.GetRecipientAsync(recipientId, cancellationToken);

            if (recipient.ManagerId != managerId)
                throw new NoAccessException($"User {managerId} is the not the manager of recipient {recipient.Id}.");

            await imageService.DeleteImageAsync(recipient.AvatarPath);

            await recipientRepository.RemoveRecipientAsync(recipient, cancellationToken);
        }

        public async Task<List<Recipient>> GetRecipientsAsync(long managerId, CancellationToken cancellationToken = default)
        {
            return await recipientRepository.GetActiveRecipientsByManagerAsync(managerId, cancellationToken);
        }

        public async Task<MemoryStream> GetAvatarAsync(long requesterId, long recipientId, CancellationToken cancellationToken = default)
        {
            Recipient recipient = await recipientRepository.GetRecipientAsync(recipientId, cancellationToken);

            if (requesterId != recipient.ManagerId && !await userRepository.ShareCommonCircleAsync(cancellationToken, requesterId, recipient.ManagerId))
                throw new NoAccessException($"User {requesterId} can not access this avatar.");

            return await imageService.DownloadImageAsync(recipient.AvatarPath);
        }
    }
}
