using CherAmiAPI.Interfaces;
using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Shared.Requests;
using CherAmiAPI.Shared.Responses;
using CherAmiAPI.Shared.SharedMappers;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;


namespace CherAmiAPI.Endpoints.Circles
{
    public class CreateCircleRequest
    {
        public string Title { get; set; }
        public IFormFile Image { get; set; }
    }

    public class CreateCircleRequestValidator : Validator<CreateCircleRequest>
    {
        public CreateCircleRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

            RuleFor(x => x.Image)
                    .Must(file => file.Length > 0).WithMessage("Image cannot be empty.");
        }
    }

    public class CreateCircleEndpoint(ApplicationDbContext ctx, IImageService imageService, IInviteCodeService inviteCodeService) : Endpoint<CreateCircleRequest, CircleDTO, CircleResponseMapper>
    {
        public override void Configure()
        {
            Post("/circle");
            AllowFileUploads();
        }

        public override async Task HandleAsync(CreateCircleRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            User user = await ctx.Users.Where(x => x.Id == userId).SingleAsync(cancellationToken: cancellationToken);

            await using var transaction = await ctx.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                string code = await inviteCodeService.GenerateCodeAsync();

                Circle toCreate = new()
                {
                    Title = request.Title,
                    TimeOfCreation = DateTimeOffset.UtcNow,
                    CircleCode = code,
                    IssueSchedule = IssueSchedule.Monthly,
                };

                ctx.Circles.Add(toCreate);
                await ctx.SaveChangesAsync(cancellationToken);

                string path = $"circles/{toCreate.Id}/header/header.jpg";

                using var stream = new MemoryStream();
                await request.Image.CopyToAsync(stream, cancellationToken);

                toCreate.HeaderPath = path;
                toCreate.HeaderTimestamp = DateTimeOffset.UtcNow;

                await imageService.UploadImageAsync(path, stream);

                Issue firstIssue = new()
                {
                    CircleId = toCreate.Id,
                    Title = "Issue 1",
                    IssueNumber = 0,
                    DraftingStart = DateTimeOffset.UtcNow,
                    DraftingEnd = new DateTimeOffset(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(2).AddTicks(-1), TimeSpan.Zero),
                    Status = IssueStatus.Drafting,
                    HeaderPath = null,
                };

                ctx.Issues.Add(firstIssue);
                user.CircleId = toCreate.Id;
                await ctx.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                await Send.CreatedAtAsync<GetCircleEndpoint>(new IdRequest() { Id = toCreate.Id }, Map.FromEntity(toCreate), cancellation: cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}