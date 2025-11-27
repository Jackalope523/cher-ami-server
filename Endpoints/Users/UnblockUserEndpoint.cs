using CherAmiAPI.Contexts;
using CherAmiAPI.Exceptions;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Shared.Requests;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Users
{
    public class UnblockUserEndpoint(ApplicationDbContext ctx, IImageService imageService) : Endpoint<IdRequest>
    {
        public override void Configure()
        {
            Delete("/users/{id}/block");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (userId == request.Id)
                throw new NoPermissionException($"A user can not unblock themselves.");

            int rowsDeleted = await ctx.Blocks
                              .Where(x => x.BlockerId == userId && x.BlockedId == request.Id)
                              .ExecuteDeleteAsync(cancellationToken: cancellationToken);

            if (rowsDeleted == 0)
                throw new NotFoundException($"Could not find a block on that user.");

            await Send.NoContentAsync(cancellationToken);
        }
    }
}