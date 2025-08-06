using FastEndpoints;
using Frontier.Contracts.Requests;
using LazyLizardBackend.Contracts.Responses;
using Mappers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class GetMembers(ICircleService circles) : Endpoint<CircleIdRequest, List<CircleMembershipShard>, CircleMembershipResponseMapper>
    {
        public override void Configure()
        {
            Get("/circle/{circleId}/members");
        }

        public override async Task HandleAsync(CircleIdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<CoreCircleMembership> coreCircleMemberships = await circles.GetCircleMembers(userId, request.CircleId);

            await Send.OkAsync(coreCircleMemberships.Select(Map.FromEntity).ToList(), cancellationToken);
        }
    }
}