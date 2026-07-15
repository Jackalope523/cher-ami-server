using CherAmiAPI.Entities;
using CherAmiAPI.Exceptions;
using CherAmiAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Services
{
    public class CircleService(
        ICircleRepository circleRepository,
        IRecipientRepository recipientRepository,
        IUserRepository userRepository,
        IImageService imageService,
        IInviteCodeService inviteCodeService,
        IUnitOfWork unitOfWork)
    {
        public async Task<Circle> CreateCircleAsync(long userId, string title, IFormFile headerImage = null, CancellationToken cancellationToken = default)
        {
            Circle toCreate = null;

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                string code = await inviteCodeService.GenerateCodeAsync();

                toCreate = new Circle
                {
                    Title = title,
                    TimeOfCreation = DateTimeOffset.UtcNow,
                    CircleCode = code,
                    IssueSchedule = IssueSchedule.Monthly,
                };

                await circleRepository.AddCircleAsync(toCreate, cancellationToken);

                if (headerImage != null)
                {
                    string path = $"circles/{toCreate.Id}/header/header.jpg";

                    using var stream = new MemoryStream();
                    await headerImage.CopyToAsync(stream, cancellationToken);

                    await circleRepository.SetHeaderAsync(toCreate.Id, path, DateTimeOffset.UtcNow, cancellationToken);

                    await imageService.UploadImageAsync(path, stream);
                }

                DateTime now = DateTime.UtcNow;
                DateTime endOfMonth = new(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
                TimeSpan untilEnd = endOfMonth - now;
                bool lessThan7DaysUntilMonthEnd = untilEnd.TotalDays < 7;

                DateTime draftingEnd = lessThan7DaysUntilMonthEnd
                    ? new DateTime(now.Year, now.Month, 1).AddMonths(2).AddTicks(-1)
                    : new DateTime(now.Year, now.Month, 1).AddMonths(1).AddTicks(-1);

                Issue firstIssue = new()
                {
                    CircleId = toCreate.Id,
                    Title = $"{draftingEnd:MMMM yyyy} · Issue 1",
                    IssueNumber = 1,
                    DraftingStart = DateTimeOffset.UtcNow,
                    DraftingEnd = new DateTimeOffset(draftingEnd, TimeSpan.Zero),
                    Status = IssueStatus.Drafting,
                    HeaderPath = null,
                };

                await circleRepository.AddIssueAsync(firstIssue, cancellationToken);
                await circleRepository.AddUserToCircleAsync(userId, toCreate.Id, cancellationToken);
            }, cancellationToken);

            return toCreate;
        }

        public async Task<Circle> GetCircleAsync(long userId, CancellationToken cancellationToken = default)
        {
            long? circleId = await circleRepository.GetCircleIdOfUserAsync(userId, cancellationToken);

            if (circleId == null)
                return null;

            List<long> blacklist = await userRepository.GetBlacklistedUserIdsAsync(userId, cancellationToken);

            return await circleRepository.GetCircleWithContributorsAsync(circleId.Value, blacklist, cancellationToken);
        }

        public async Task UpdateCircleAsync(long userId, string title, IFormFile header = null, CancellationToken cancellationToken = default)
        {
            Circle circle = await circleRepository.GetCircleOfUserAsync(userId, cancellationToken);

            string headerPath = null;
            DateTimeOffset? headerTimestamp = null;

            if (header != null)
            {
                using MemoryStream stream = new();
                await header.CopyToAsync(stream, cancellationToken);

                headerPath = $"circles/{circle.Id}/header/header.jpg";
                headerTimestamp = DateTimeOffset.UtcNow;

                await imageService.UploadImageAsync(headerPath, stream);
            }

            await circleRepository.UpdateCircleAsync(circle.Id, title, headerPath, headerTimestamp, cancellationToken);
        }

        public async Task UpdateHeaderAsync(long userId, IFormFile image, CancellationToken cancellationToken = default)
        {
            Circle circle = await circleRepository.GetCircleOfUserAsync(userId, cancellationToken);

            using var stream = new MemoryStream();
            await image.CopyToAsync(stream, cancellationToken);

            string path = $"circles/{circle.Id}/header/header.jpg";

            await imageService.UploadImageAsync(path, stream);
            await circleRepository.SetHeaderAsync(circle.Id, path, DateTimeOffset.UtcNow, cancellationToken);
        }

        public async Task<MemoryStream> GetHeaderAsync(long userId, long circleId, CancellationToken cancellationToken = default)
        {
            if (!await circleRepository.IsUserInCircleAsync(userId, circleId, cancellationToken))
                throw new NoAccessException($"User {userId} can not access this header.");

            string path = await circleRepository.GetHeaderPathAsync(circleId, cancellationToken);

            return await imageService.DownloadImageAsync(path);
        }

        public async Task<string> GetCodeAsync(long userId, CancellationToken cancellationToken = default)
        {
            return await circleRepository.GetCircleCodeOfUserAsync(userId, cancellationToken);
        }

        public async Task<string> RerollCodeAsync(long userId, CancellationToken cancellationToken = default)
        {
            Circle circle = await circleRepository.GetCircleOfUserAsync(userId, cancellationToken);

            string code = await inviteCodeService.GenerateCodeAsync();
            await circleRepository.SetCircleCodeAsync(circle.Id, code, cancellationToken);

            return code;
        }

        public async Task JoinCircleAsync(long userId, string code, CancellationToken cancellationToken = default)
        {
            long? currentCircleId = await circleRepository.GetCircleIdOfUserAsync(userId, cancellationToken);

            if (currentCircleId != null)
                throw new NoPermissionException($"User {userId} already has a circle.");

            long circleId = await circleRepository.GetCircleIdByCodeAsync(code, cancellationToken);

            if (circleId == 0)
                throw new NotFoundException($"Invalid invite code.");

            await circleRepository.AddUserToCircleAsync(userId, circleId, cancellationToken);
        }

        public async Task LeaveCircleAsync(long userId, CancellationToken cancellationToken = default)
        {
            await circleRepository.RemoveUserFromCircleAsync(userId, cancellationToken);

            List<string> recipientAvatars = await recipientRepository.GetAvatarPathsByManagerAsync(userId, cancellationToken);
            await imageService.DeleteImagesAsync(recipientAvatars);

            await recipientRepository.DeleteRecipientsOfManagerAsync(userId, cancellationToken);
        }
    }
}
