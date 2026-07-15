using CherAmiAPI.Entities;
using CherAmiAPI.Services;
using CherAmiAPI.Shared.Mappers;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Recipients
{
    public class GetRecipientsEndpoint(RecipientService recipientService) : EndpointWithoutRequest<List<RecipientItem>, RecipientItemMapper>
    {
        public override void Configure()
        {
            Get("/recipients");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<Recipient> recipients = await recipientService.GetRecipientsAsync(userId, cancellationToken);

            await Send.OkAsync([.. recipients.Select(Map.FromEntity)], cancellationToken);
        }
    }
}
