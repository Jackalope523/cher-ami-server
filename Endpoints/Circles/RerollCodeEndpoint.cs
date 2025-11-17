using CherAmiAPI.Interfaces;
using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Circles
{
    public class RerollCodeEndpoint(ApplicationDbContext ctx, IInviteCodeService inviteCodeService) : EndpointWithoutRequest<CodeResponse>
    {
        public override void Configure()
        {
            Post("/circle/code");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Circle circle = await ctx.Users.Where(x => x.Id == userId).Select(x => x.Circle).SingleAsync(cancellationToken: cancellationToken);

            circle.CircleCode = await inviteCodeService.GenerateCodeAsync();
            await ctx.SaveChangesAsync(cancellationToken);

            CodeResponse response = new()
            {
                Code = circle.CircleCode
            };

            await Send.OkAsync(response, cancellationToken);
        }
    }
}