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
    public class GetRecipientEndpoint(RecipientService recipientService) : Endpoint<IdRequest, RecipientDTO, RecipientMapper>
    {
        public override void Configure()
        {
            Get("/circle/recipients/{id}");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Recipient recipient = await recipientService.GetRecipientAsync(userId, request.Id, cancellationToken);

            await Send.OkAsync(Map.FromEntity(recipient), cancellation: cancellationToken);
        }
    }
}
