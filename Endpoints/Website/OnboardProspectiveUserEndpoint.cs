using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Services;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Website
{
    public class OnboardProspectiveUserRequest
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> FriendEmails { get; set; } = [];
        public string Caption { get; set; }
        public IFormFile Image { get; set; }
    }

    public class OnboardProspectiveUserRequestValidator : Validator<OnboardProspectiveUserRequest>
    {
        public OnboardProspectiveUserRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be valid.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

            RuleFor(x => x.Caption)
                .MaximumLength(200).WithMessage("Caption cannot exceed 200 characters.");

            RuleForEach(x => x.FriendEmails)
                .EmailAddress().WithMessage("All friend emails must be valid.");
        }
    }

    public class OnboardProspectiveUserResponse
    {
        public Guid ExternalId { get; set; }
    }

    public class OnboardProspectiveUserEndpoint(
        ApplicationDbContext ctx,
        IKeyService keyService,
        OneSignalService oneSignalService,
        UserManager<User> userManager,
        CircleService circleService,
        IImageService imageService) : Endpoint<OnboardProspectiveUserRequest, OnboardProspectiveUserResponse>
    {
        public override void Configure()
        {
            Post("/website/onboarding");
            AllowAnonymous();
            AllowFileUploads();
        }

        public override async Task HandleAsync(OnboardProspectiveUserRequest request, CancellationToken cancellationToken)
        {
            string apiKey = HttpContext.Request.Headers["Authorization"].ToString()["key ".Length..];
            if (apiKey != await keyService.GetSecretAsync("Cher-Ami-API-Key"))
            {
                await Send.ForbiddenAsync(cancellationToken);
                return;
            }

            // 1. Get or create primary user
            User user = await ctx.Users
                .Where(u => u.Email == request.Email)
                .FirstOrDefaultAsync(cancellationToken);

            if (user != null)
            {
                await Send.NoContentAsync(cancellationToken);
                return;
            }

            user = new()
            {
                UserName = request.Email,
                Email = request.Email,
                ExternalId = Guid.NewGuid(),
                AccountStatus = UserAccountStatus.Prospective,
                FirstName = request.FirstName,
                LastName = request.LastName,
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

            // 2. Create circle (also creates first issue and links user)
            Circle circle = await circleService.CreateCircleAsync(user.Id, $"{user.FirstName}'s Circle", cancellationToken: cancellationToken);

            // 3. Get the first issue from the new circle
            long issueId = await ctx.Issues
                .Where(x => x.CircleId == circle.Id)
                .Select(x => x.Id)
                .FirstAsync(cancellationToken);

            // 4. Get or create each friend and associate with circle
            foreach (string friendEmail in request.FriendEmails)
            {
                User friend = await ctx.Users
                    .Where(u => u.Email == friendEmail)
                    .FirstOrDefaultAsync(cancellationToken);

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
                    await oneSignalService.AddTagAsync(friend.ExternalId, "email_reminders", "1", cancellationToken);
                    await oneSignalService.AddTagAsync(friend.ExternalId, "email_marketing", "1", cancellationToken);
                    await oneSignalService.AddTagAsync(friend.ExternalId, "invited_by", $"{user.FirstName} {user.LastName}", cancellationToken);

                    var result = await userManager.CreateAsync(friend);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                            Log.Error("Error creating friend user {Email}: {Error}", friendEmail, error.Description);

                        continue;
                    }

                    friend.CircleId = circle.Id;
                    friend.CircleJoinDate = DateTimeOffset.UtcNow;
                }
                // Friend already exists — leave their data untouched
            }

            await ctx.SaveChangesAsync(cancellationToken);

            // 5. Optionally process and upload the post image
            // 5. Optionally create a post
            if (request.Image != null)
            {
                await using var transaction = await ctx.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    Post post = new()
                    {
                        AuthorId = user.Id,
                        IssueId = issueId,
                        Caption = request.Caption,
                        PostedAt = DateTimeOffset.UtcNow,
                    };

                    ctx.Posts.Add(post);
                    await ctx.SaveChangesAsync(cancellationToken);

                    using var stream = new MemoryStream();
                    await request.Image.CopyToAsync(stream, cancellationToken);

                    string path = $"circles/{circle.Id}/issues/{issueId}/posts/{post.Id}/{Guid.NewGuid()}.jpg";
                    post.HighResolutionImagePath = path;
                    post.LowResolutionImagePath = path;
                    await ctx.SaveChangesAsync(cancellationToken);

                    await imageService.UploadImageAsync(path, stream);

                    await transaction.CommitAsync(cancellationToken);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }

            await Send.OkAsync(new OnboardProspectiveUserResponse { ExternalId = user.ExternalId }, cancellationToken);
        }
    }
}
