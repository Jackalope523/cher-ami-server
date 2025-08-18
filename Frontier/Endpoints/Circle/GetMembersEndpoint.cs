using Core.Boundaries;
using FastEndpoints;
using LazyLizardBackend.Contracts.Requests;
using LazyLizardBackend.Contracts.Responses;
using LazyLizardBackend.Shared.SharedMappers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class GetMembersEndpoint(ICircleService circles) : Endpoint<IdRequest, List<CircleMembershipDTO>, CircleMembershipResponseMapper>
    {
        public override void Configure()
        {
            Get("/circle/{circleId}/members");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<CoreCircleMembership> coreCircleMemberships = await circles.GetCircleMembers(userId, request.Id);

            await Send.OkAsync(coreCircleMemberships.Select(Map.FromEntity).ToList(), cancellationToken);
        }
    }
}