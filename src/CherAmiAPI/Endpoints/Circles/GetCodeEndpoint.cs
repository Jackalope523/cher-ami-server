using CherAmiAPI.Services;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Circles
{
    public class GetCodeEndpoint(CircleService circleService) : EndpointWithoutRequest<CodeResponse>
    {
        public override void Configure()
        {
            Get("/circle/code");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            string circleCode = await circleService.GetCodeAsync(userId, cancellationToken);

            CodeResponse response = new()
            {
                Code = circleCode,
            };

            await Send.OkAsync(response, cancellationToken);
        }
    }
}
