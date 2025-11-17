using CherAmiAPI.Exceptions;
using CherAmiAPI.Shared.Responses;
using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Shared.Mappers;
using CherAmiAPI.Shared.Requests;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Circles
{
    public class GetRecipientEndpoint(ApplicationDbContext ctx) : Endpoint<IdRequest, RecipientDTO, RecipientMapper>
    {
        public override void Configure()
        {
            Get("/circle/recipients/{id}");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Recipient recipient = await ctx.Recipients.Where(x => x.Id == request.Id).SingleAsync(cancellationToken: cancellationToken);

            if (userId != recipient.ManagerId)
            {
                int count = await ctx.Users.Where(x => x.Id == userId || x.Id == recipient.ManagerId).Select(x => x.CircleId).Distinct().CountAsync(cancellationToken: cancellationToken);

                if (count > 1)
                    throw new NoAccessException($"User {userId} can not access this recipient.");
            }


            await Send.OkAsync(Map.FromEntity(recipient), cancellation: cancellationToken);
        }
    }
}
