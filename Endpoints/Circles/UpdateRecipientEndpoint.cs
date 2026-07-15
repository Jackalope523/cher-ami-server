using CherAmiAPI.Services;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
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
        public string Name { get; set; }
        public string AddressLine1 { get; init; }
        public string AddressLine2 { get; init; }
        public string City { get; init; }
        public string ProvinceOrState { get; init; }
        public string PostalCode { get; init; }
        public string Country { get; init; }
        public bool? IsVeteran { get; init; }
    }

    public class UpdateRecipientRequestValidator : Validator<UpdateRecipientRequest>
    {
        public UpdateRecipientRequestValidator()
        {

        }
    }

    public class UpdateRecipientEndpoint(RecipientService recipientService) : Endpoint<UpdateRecipientRequest>
    {
        public override void Configure()
        {
            Put("/circle/recipients/{id}");
            AllowFileUploads();
        }

        public override async Task HandleAsync(UpdateRecipientRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await recipientService.UpdateRecipientAsync(
                userId,
                request.Id,
                request.Title,
                request.Name,
                request.AddressLine1,
                request.AddressLine2,
                request.City,
                request.ProvinceOrState,
                request.PostalCode,
                request.Country,
                request.IsVeteran,
                request.Avatar,
                cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
