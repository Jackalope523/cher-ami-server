using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Interfaces;
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

namespace CherAmiAPI.Endpoints.Circles
{
    public class UpdateCircleRequest
    {
        public string Title { get; set; }
        public IFormFile Header { get; set; }
    }

    public class UpdateCircleRequestValidator : Validator<UpdateCircleRequest>
    {
        public UpdateCircleRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

            RuleFor(x => x.Header)
                .Must(x => x.ContentType == "image/jpeg" || x.ContentType == "image/jpg").WithMessage("Image must be a jpeg.")
                .Must(x => x.Length > 0).WithMessage("Image cannot be empty.")
                .Must(x => x.Length <= 5 * 1024 * 1024).WithMessage("Image cannot exceed 5MB.")
                .When(x => x.Header != null);
        }
    }

    public class UpdateCircleEndpoint(ApplicationDbContext ctx, IImageService imageService) : Endpoint<UpdateCircleRequest>
    {
        public override void Configure()
        {
            Put("/circle");
            AllowFileUploads();
        }

        public override async Task HandleAsync(UpdateCircleRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Circle circle = await ctx.Users
                .Where(x => x.Id == userId)
                .Select(x => x.Circle)
                .SingleAsync(cancellationToken: cancellationToken);

            await using var transaction = await ctx.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                circle.Title = request.Title;

                if (request.Header != null)
                {
                    using MemoryStream stream = new();
                    await request.Header.CopyToAsync(stream, cancellationToken);

                    string path = $"circles/{circle.Id}/header/header.jpg";

                    circle.HeaderPath = path;
                    circle.HeaderTimestamp = DateTimeOffset.UtcNow;

                    await imageService.UploadImageAsync(path, stream);
                }

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
