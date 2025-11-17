using CherAmiAPI.Interfaces;
using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Users
{
    public class UpdateUserRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public IFormFile Avatar { get; set; }
    }

    public class UpdateUserRequestValidator : Validator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100).WithMessage("Last name cannot exceed 50 characters.");

            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required.")
                .Must(x => x < today.AddYears(-13) && x > today.AddYears(-110)).WithMessage("Invalid date of birth.");

            RuleFor(x => x.Avatar)
                .NotNull().WithMessage("Avatar is required.")
                .Must(x => x.ContentType == "image/jpeg" || x.ContentType == "image/jpg").WithMessage("Image must be a jpeg.")
                .Must(x => x.Length > 0).WithMessage("Image can not be empty.")
                .Must(x => x.Length <= 5 * 1024 * 1024).WithMessage("Image cannot exceed 5MB.");
        }
    }

    public class UpdateUserEndpoint(ApplicationDbContext ctx, IImageService imageService) : Endpoint<UpdateUserRequest>
    {
        public override void Configure()
        {
            Put("/user");
            AllowFileUploads();
        }

        public override async Task HandleAsync(UpdateUserRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await using var transaction = await ctx.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                User user = await ctx.Users.Where(x => x.Id == userId).SingleAsync(cancellationToken: cancellationToken);

                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.DateOfBirth = request.DateOfBirth;
                user.JoinDate = DateTimeOffset.UtcNow;

                using var stream = new MemoryStream();
                await request.Avatar.CopyToAsync(stream, cancellationToken);

                string path = $"users/{user.Id}/avatar.jpg";

                user.AvatarPath = path;
                user.AvatarTimestamp = DateTimeOffset.UtcNow;

                await imageService.UploadImageAsync(path, stream);

                await ctx.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await Send.NoContentAsync(cancellationToken);
            }
            catch (Exception)
            {
               await transaction.RollbackAsync(cancellationToken);
               throw;
            }
        }
    }
}