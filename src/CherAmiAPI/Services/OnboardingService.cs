using CherAmiAPI.Entities;
using CherAmiAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Services
{
    public class OnboardingService(
        IUserRepository userRepository,
        IPostRepository postRepository,
        UserManager<User> userManager,
        IOneSignalService oneSignalService,
        CircleService circleService,
        IImageService imageService,
        IConfiguration config)
    {
        public async Task<(Guid ExternalId, string OneSignalId)> GetOrCreateProspectiveUserAsync(string email, CancellationToken cancellationToken = default)
        {
            User user = await userRepository.FindUserByEmailAsync(email, cancellationToken);

            if (user != null)
                return (user.ExternalId, user.OneSignalId);

            User newUser = new()
            {
                UserName = email,
                Email = email,
                ExternalId = Guid.NewGuid(),
                AccountStatus = UserAccountStatus.Prospective,
            };

            newUser.OneSignalId = await oneSignalService.CreateUserAsync(newUser.ExternalId, newUser.Email, cancellationToken);
            await oneSignalService.AddTagAsync(newUser.ExternalId, "email_reminders", "1", cancellationToken);
            await oneSignalService.AddTagAsync(newUser.ExternalId, "email_marketing", "1", cancellationToken);

            var result = await userManager.CreateAsync(newUser);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    Log.Error("Error creating user: {Error}", error.Description);
                }
            }

            return (newUser.ExternalId, newUser.OneSignalId);
        }

        public async Task<Guid?> OnboardProspectiveUserAsync(
            string email,
            string firstName,
            string lastName,
            List<string> friendEmails,
            string recipientName,
            string caption,
            IFormFile image,
            CancellationToken cancellationToken = default)
        {
            User user = await userRepository.FindUserByEmailAsync(email, cancellationToken);

            if (user != null)
                return null;

            user = new()
            {
                UserName = email,
                Email = email,
                ExternalId = Guid.NewGuid(),
                AccountStatus = UserAccountStatus.Prospective,
                FirstName = firstName,
                LastName = lastName,
            };

            user.OneSignalId = await oneSignalService.CreateUserAsync(user.ExternalId, user.Email, cancellationToken);
            await oneSignalService.AddTagAsync(user.ExternalId, "email_reminders", "1", cancellationToken);
            await oneSignalService.AddTagAsync(user.ExternalId, "email_marketing", "1", cancellationToken);

            var result = await userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    Log.Error("Error creating user: {Error}", error.Description);

                throw new Exception("Failed to create prospective user.");
            }

            // Creating the circle also creates its first issue and links the user
            Circle circle = await circleService.CreateCircleAsync(user.Id, $"{user.FirstName}'s Circle", cancellationToken: cancellationToken);

            long issueId = await postRepository.GetFirstIssueIdOfCircleAsync(circle.Id, cancellationToken);

            List<string> newFriendEmails = [];
            foreach (string friendEmail in friendEmails)
            {
                User friend = await userRepository.FindUserByEmailAsync(friendEmail, cancellationToken);

                if (friend == null)
                {
                    friend = new()
                    {
                        UserName = friendEmail,
                        Email = friendEmail,
                        ExternalId = Guid.NewGuid(),
                        AccountStatus = UserAccountStatus.Prospective,
                    };

                    friend.OneSignalId = await oneSignalService.CreateUserAsync(friend.ExternalId, friend.Email, cancellationToken);
                    await oneSignalService.AddTagAsync(friend.ExternalId, "email_reminders", "0", cancellationToken);
                    await oneSignalService.AddTagAsync(friend.ExternalId, "email_marketing", "0", cancellationToken);
                    await oneSignalService.AddTagAsync(friend.ExternalId, "invited_by", $"{user.FirstName} {user.LastName}", cancellationToken);

                    var friendResult = await userManager.CreateAsync(friend);

                    if (!friendResult.Succeeded)
                    {
                        foreach (var error in friendResult.Errors)
                            Log.Error("Error creating friend user {Email}: {Error}", friendEmail, error.Description);

                        continue;
                    }

                    friend.CircleId = circle.Id;
                    friend.CircleJoinDate = DateTimeOffset.UtcNow;
                    newFriendEmails.Add(friendEmail);
                }
                // Friend already exists — leave their data untouched
            }

            await userRepository.SaveUserAsync(user, cancellationToken);

            await oneSignalService.TrackEventAsync(user.ExternalId, "onboarding_started", cancellationToken);

            // Send welcome email to all newly invited friends
            if (newFriendEmails.Count > 0)
            {
                await oneSignalService.SendTemplatedEmailAsync(
                    config["ONESIGNAL_INVITEE_WELCOME_EMAIL_TEMPLATE_ID"],
                    newFriendEmails,
                    new
                    {
                        inviter = $"{user.FirstName} {user.LastName}",
                        recipient_name = recipientName,
                    },
                    cancellationToken);
            }

            // Optionally create a post
            if (image != null)
            {
                // Soft-deleted until the image is in blob storage, so a failed upload never leaves a broken post in the feed
                Post post = new()
                {
                    AuthorId = user.Id,
                    IssueId = issueId,
                    Caption = caption ?? "",
                    PostedAt = DateTimeOffset.UtcNow,
                    ImageWidth = 1088,
                    ImageHeight = 756,
                    SoftDeleted = true,
                };

                await postRepository.AddPostAsync(post, cancellationToken);

                using var stream = new MemoryStream();
                await image.CopyToAsync(stream, cancellationToken);

                string path = $"circles/{circle.Id}/issues/{issueId}/posts/{post.Id}/{Guid.NewGuid()}.jpg";
                await imageService.UploadImageAsync(path, stream);

                post.HighResolutionImagePath = path;
                post.LowResolutionImagePath = path;
                post.SoftDeleted = false;
                await postRepository.SavePostAsync(post, cancellationToken);
            }

            return user.ExternalId;
        }
    }
}
