using CherAmiAPI.Entities;
using CherAmiAPI.Services;
using CherAmiAPI.Shared.Mappers;
using CherAmiAPI.Shared.Requests;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Circles
{

    public class AddRecipientEndpoint(RecipientService recipientService) : Endpoint<RecipientRequest, RecipientDTO, RecipientMapper>
    {
        public override void Configure()
        {
            Post("/circle/recipients");
            AllowFileUploads();
        }

        public override async Task HandleAsync(RecipientRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Recipient toAdd = Map.ToEntity(request);

            await recipientService.AddRecipientAsync(userId, toAdd, request.Avatar, cancellationToken);

            await Send.CreatedAtAsync<GetRecipientEndpoint>
            (
                new IdRequest() { Id = toAdd.Id },
                Map.FromEntity(toAdd),
                cancellation: cancellationToken
            );
        }
    }
}
