using CherAmiAPI.Exceptions;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Contexts;
using CherAmiAPI.Endpoints.Media;
using CherAmiAPI.Entities;
using CherAmiAPI.Shared.Mappers;
using CherAmiAPI.Shared.Responses;
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
    public class UpdateRecipientRequest
    {
        public long Id { get; init; }
        public IFormFile Avatar { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Street { get; init; }
        public string City { get; init; }
        public string ProvinceOrState { get; init; }
        public string PostalCode { get; init; }
        public string Country { get; init; }
        public string UnitNumber { get; init; }
    }

    public class UpdateRecipientRequestValidator : Validator<UpdateRecipientRequest>
    {
        public UpdateRecipientRequestValidator()
        {
  
        }
    }

    public class UpdateRecipientEndpoint(ApplicationDbContext ctx, IImageService imageService) : Endpoint<UpdateRecipientRequest>
    {
        public override void Configure()
        {
            Put("/circle/recipients/{id}");
            AllowFileUploads();
        }

        public override async Task HandleAsync(UpdateRecipientRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Recipient recipient = await ctx.Recipients.Where(x => x.Id == request.Id).SingleAsync(cancellationToken: cancellationToken);

            if (recipient.ManagerId != userId) 
                throw new NoAccessException();

            await using var transaction = await ctx.Database.BeginTransactionAsync(cancellationToken);

            try
            {


                recipient.Title = request.Title;
                recipient.FirstName = request.FirstName;
                recipient.LastName = request.LastName;
                recipient.Street = request.Street;
                recipient.City = request.City;
                recipient.ProvinceOrState = request.ProvinceOrState;
                recipient.Country = request.Country;
                recipient.UnitNumber = request.UnitNumber;
                recipient.PostalCode = request.PostalCode;

                if (request.Avatar != null)
                {
                    await imageService.DeleteImageAsync(recipient.AvatarPath);

                    using var stream = new MemoryStream();
                    await request.Avatar.CopyToAsync(stream, cancellationToken);

                    string path = $"users/{userId}/recipients/{request.Id}/avatar.jpg";

                    recipient.AvatarPath = path;
                    recipient.AvatarTimestamp = DateTimeOffset.UtcNow;

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