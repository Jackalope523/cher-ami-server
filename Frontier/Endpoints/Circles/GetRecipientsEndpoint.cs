using FastEndpoints;
using CrazyLizard.Shared.SharedMappers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Shared.Responses;
using CrazyLizard.Interfaces.Service;
using CrazyLizard.Entities;

namespace CrazyLizard.Endpoints.Circles
{
    public class GetRecipientsEndpoint(ICircleService circles) : EndpointWithoutRequest<List<RecipientDTO>, RecipientResponseMapper>
    {
        public override void Configure()
        {
            Get("/circle/recipients");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<Recipient> coreRecipients = await circles.GetRecipientsForCircleAsync(userId);
            await Send.OkAsync(coreRecipients.Select(Map.FromEntity).ToList(), cancellationToken);
        }
    }
}
