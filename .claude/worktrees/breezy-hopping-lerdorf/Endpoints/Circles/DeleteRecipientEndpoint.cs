using CherAmiAPI.Interfaces;
using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Shared.Requests;
using FastEndpoints;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Issues
{
    public class DeleteRecipientEndpoint(ApplicationDbContext ctx, IImageService imageService) : Endpoint<IdRequest>
    {
        public override void Configure()
        {
            Delete("/circle/recipients/{id}");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Recipient recipient = await ctx.Recipients.FindAsync(request.Id);

            if (recipient.ManagerId != userId) 
                throw new CherAmiAPI.Exceptions.NoAccessException($"User {userId} is the not the manager of recipient {recipient.Id}.");

            //await using var transaction = await ctx.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await imageService.DeleteImageAsync(recipient.AvatarPath);

                ctx.Recipients.Remove(recipient);
                await ctx.SaveChangesAsync(cancellationToken);

                //await transaction.CommitAsync(cancellationToken);

                await Send.NoContentAsync(cancellationToken);
            }
            catch (Exception)
            {
                //await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}