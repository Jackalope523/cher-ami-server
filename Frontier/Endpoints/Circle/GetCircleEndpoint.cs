using Core.Boundaries;
using FastEndpoints;
using CrazyLizard.Contracts.Requests;
using CrazyLizard.Contracts.Responses;
using CrazyLizard.Shared.SharedMappers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class GetCircleEndpoint(ICircleService circles) : Endpoint<IdRequest, CircleDTO, CircleResponseMapper>
    {
        public override void Configure()
        {
            Get("/circle/{circleId}");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            CoreCircle coreCircle = await circles.GetCircleInformationAsync(userId, request.Id);

            if (coreCircle == null) 
                await Send.NotFoundAsync(cancellationToken);
            
            await Send.OkAsync(Map.FromEntity(coreCircle), cancellationToken);
        }
    }
}