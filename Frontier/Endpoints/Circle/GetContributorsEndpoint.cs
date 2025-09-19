using Core.Boundaries;
using FastEndpoints;
using CrazyLizard.Shared.SharedMappers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Frontier.Contracts.Responses;
using CrazyLizard.Entities;

namespace Frontier.Endpoints.Account
{
    public class GetContributorsEndpoint(ICircleService circles) : EndpointWithoutRequest<List<UserDTO>, UserResponseMapper>
    {
        public override void Configure()
        {
            Get("/circle/contributors");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<User> coreCircleMemberships = await circles.GetCircleMembers(userId);

            await Send.OkAsync(coreCircleMemberships.Select(Map.FromEntity).ToList(), cancellationToken);
        }
    }
}