using CrazyLizard.Contexts;
using CrazyLizard.Shared.Responses;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Circles
{
    public class GetCodeEndpoint(ApplicationDbContext ctx) : EndpointWithoutRequest<CodeResponse>
    {
        public override void Configure()
        {
            Get("/circle/code");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            string circleCode = await ctx.Users.Where(x => x.Id == userId).Select(x => x.Circle.CircleCode).SingleAsync(cancellationToken: cancellationToken);

            CodeResponse response = new()
            {
                Code = circleCode,
            };

            await Send.OkAsync(response, cancellationToken);
        }
    }
}