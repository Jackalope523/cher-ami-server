using FastEndpoints;
using Frontier.Contracts.Requests;
using Frontier.Contracts.Responses;
using LazyLizardBackend.Shared.SharedMappers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace LazyLizardBackend.Endpoints.Circle
{
    public class GetRecipientsEndpoint(ICircleService circles) : Endpoint<CircleIdRequest, List<RecipientDTO>, RecipientResponseMapper>
    {
        public override void Configure()
        {
            Get("/circle/{circleId}/recipients");
        }

        public override async Task HandleAsync(CircleIdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<CoreRecipient> coreRecipients = await circles.GetRecipientsForCircleAsync(userId, request.Id);
            await Send.OkAsync(coreRecipients.Select(Map.FromEntity).ToList());
        }
    }
}
