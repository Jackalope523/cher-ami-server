using CrazyLizard.Contexts;
using CrazyLizard.Entities;
using CrazyLizard.Interfaces.Service;
using CrazyLizard.Shared.Mappers;
using CrazyLizard.Shared.Requests;
using CrazyLizard.Shared.Responses;
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

namespace CrazyLizard.Endpoints.Issues
{
    public class CreatePostRequest
    {
        public DateTime Time { get; set; }
        public string Caption { get; set; }
        public IFormFile Image { get; set; }
    }

    public class CreatePostRequestValidator : Validator<CreatePostRequest>
    {
        public CreatePostRequestValidator()
        {
            RuleFor(x => x.Time)
                .NotEmpty().WithMessage("Time is required.");

            RuleFor(x => x.Image)
                .NotNull().WithMessage("Image is required.")
                .Must(file => file.Length > 0).WithMessage("Uploaded image can not be empty.");

            RuleFor(x => x.Caption)
                .MaximumLength(200).WithMessage("Caption cannot exceed 200 characters.");
        }
    }

    public class AddPostEndpoint(ApplicationDbContext ctx, IImageService imageService) : Endpoint<CreatePostRequest, PostDTO, PostResponseMapper>
    {
        public override void Configure()
        {
            Post("/issue/posts");
            AllowFileUploads();
        }

        public override async Task HandleAsync(CreatePostRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            long? circleId = await ctx.Users.Where(x => x.Id == userId).Select(x => x.CircleId).SingleAsync(cancellationToken: cancellationToken);

            //JACKALOPE: Fucking  SQLITE DATE ORDERING SHIT.
            //long issueId = await ctx.Issues.OrderByDescending(x => x.DraftingEnd).Select(x => x.Id).FirstAsync(cancellationToken: cancellationToken);

            long currentIssueId = (await ctx.Issues
                            .Where(x => x.CircleId == circleId)
                            .ToListAsync(cancellationToken: cancellationToken))
                            .OrderByDescending(x => x.DraftingEnd)
                            .Select(x => x.Id)
                            .First();

            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                Post postToAdd = new()
                {
                    IssueId = currentIssueId,
                    AuthorId = userId,
                    PostedAt = request.Time,
                    Caption = request.Caption,
                };

                ctx.Posts.Add(postToAdd);
                await ctx.SaveChangesAsync(cancellationToken);

                using var stream = new MemoryStream();
                await request.Image.CopyToAsync(stream, cancellationToken);

                string path = $"circles/{circleId}/issues/{currentIssueId}/posts/{request.Image.Name}.jpg";

                postToAdd.ImagePath = path;
                await ctx.SaveChangesAsync(cancellationToken);

                await imageService.UploadImageAsync(path, stream);

                await transaction.CommitAsync(cancellationToken);

                await Send.CreatedAtAsync<GetPostEndpoint>
                (
                    new IdRequest() { Id = postToAdd.Id },
                    Map.FromEntity(postToAdd),
                    cancellation: cancellationToken
                );
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}