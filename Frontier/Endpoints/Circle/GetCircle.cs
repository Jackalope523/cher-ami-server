using FastEndpoints;
using Frontier.Contracts.Requests;
using LazyLizardBackend.Contracts.Responses;
using Mappers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class GetCircle(ICircleService circles) : Endpoint<CircleIdRequest, CircleDTO, CircleResponseMapper>
    {
        public override void Configure()
        {
            Get("/circle/{circleId}");
        }

        public override async Task HandleAsync(CircleIdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            CoreCircle coreCircle = await circles.GetCircleInformationAsync(userId, request.CircleId);

            if (coreCircle == null) 
                await Send.NotFoundAsync(cancellationToken);
            
            await Send.OkAsync(Map.FromEntity(coreCircle), cancellationToken);
        }
    }
}