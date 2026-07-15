using CherAmiAPI.Services;
using CherAmiAPI.Shared.Requests;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Issues
{
    public class DeleteRecipientEndpoint(RecipientService recipientService) : Endpoint<IdRequest>
    {
        public override void Configure()
        {
            Delete("/circle/recipients/{id}");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await recipientService.DeleteRecipientAsync(userId, request.Id, cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
