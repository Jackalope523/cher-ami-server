using CherAmiAPI.Interfaces;
using CrazyLizard.Contexts;
using CrazyLizard.Entities;
using CrazyLizard.Exceptions;
using CrazyLizard.Shared.Mappers;
using CrazyLizard.Shared.Requests;
using CrazyLizard.Shared.Responses;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Circles
{
    
    public class AddRecipientEndpoint(ApplicationDbContext ctx, IImageService imageService) : Endpoint<RecipientRequest, RecipientDTO, RecipientMapper>
    {
        public override void Configure()
        {
            Post("/circle/recipients");
            AllowFileUploads();
        }

        public override async Task HandleAsync(RecipientRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!await ctx.Users.Where(x => x.Id == userId).AnyAsync(x => x.ProvidedPaymentDetails))
                throw new NoPermissionException($"User {userId} has not provided payment details.");

            await using var transaction = await ctx.Database.BeginTransactionAsync();
            try
            {
                Recipient toAdd = Map.ToEntity(request);
                ctx.Recipients.Add(toAdd);
                toAdd.ManagerId = userId;
                await ctx.SaveChangesAsync(cancellationToken);

                string path = $"users/{userId}/recipients/{toAdd.Id}/avatar.jpg";
                toAdd.AvatarPath = path;
                await ctx.SaveChangesAsync(cancellationToken);

                using MemoryStream memoryStream = new();
                await request.Avatar.CopyToAsync(memoryStream);
                await imageService.UploadImageAsync(path, memoryStream);

                await transaction.CommitAsync(cancellationToken);

                await Send.CreatedAtAsync<GetRecipientEndpoint>
                (
                    new IdRequest() { Id =  toAdd.Id },
                    Map.FromEntity(toAdd),
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