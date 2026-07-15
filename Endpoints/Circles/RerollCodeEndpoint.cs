using CherAmiAPI.Services;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Circles
{
    public class RerollCodeEndpoint(CircleService circleService) : EndpointWithoutRequest<CodeResponse>
    {
        public override void Configure()
        {
            Post("/circle/code");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            string code = await circleService.RerollCodeAsync(userId, cancellationToken);

            CodeResponse response = new()
            {
                Code = code
            };

            await Send.OkAsync(response, cancellationToken);
        }
    }
}
