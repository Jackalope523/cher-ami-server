using Core.Boundaries;
using FastEndpoints;
using Frontier.Contracts.Responses;
using CrazyLizard.Contracts.Requests;
using CrazyLizard.Shared.SharedMappers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Circle
{
    public class GetRecipientsEndpoint(ICircleService circles) : Endpoint<IdRequest, List<RecipientDTO>, RecipientResponseMapper>
    {
        public override void Configure()
        {
            Get("/circle/{circleId}/recipients");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<CoreRecipient> coreRecipients = await circles.GetRecipientsForCircleAsync(userId, request.Id);
            await Send.OkAsync(coreRecipients.Select(Map.FromEntity).ToList());
        }
    }
}
