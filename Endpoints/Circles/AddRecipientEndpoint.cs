using CherAmiAPI.Contexts;
using CherAmiAPI.Endpoints.Circles;
using CherAmiAPI.Entities;
using CherAmiAPI.Exceptions;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Shared.Mappers;
using CherAmiAPI.Shared.Requests;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Stripe;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Circles
{
    
    public class AddRecipientEndpoint(ApplicationDbContext ctx, IImageService imageService, CustomerPaymentMethodService customerPaymentMethodService) : Endpoint<RecipientRequest, RecipientDTO, RecipientMapper>
    {
        public override void Configure()
        {
            Post("/circle/recipients");
            AllowFileUploads();
        }

        public override async Task HandleAsync(RecipientRequest request, CancellationToken cancellationToken)
        {
            Log.Error("HIT");
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            string userStripeCustomerId = await ctx.Users.Where(x => x.Id == userId).Select(x => x.StripeCustomerId).SingleAsync(cancellationToken: cancellationToken);

            //if ((await customerPaymentMethodService.ListAsync(userStripeCustomerId, cancellationToken: cancellationToken)).Data.Count == 0)
            //    throw new NoPermissionException($"User {userId} has not provided a payment method.");

            //await using var transaction = await ctx.Database.BeginTransactionAsync();
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

                //await transaction.CommitAsync(cancellationToken);

                await Send.CreatedAtAsync<GetRecipientEndpoint>
                (
                    new IdRequest() { Id =  toAdd.Id },
                    Map.FromEntity(toAdd),
                    cancellation: cancellationToken
                );
            }
            catch (Exception)
            {
                //await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}