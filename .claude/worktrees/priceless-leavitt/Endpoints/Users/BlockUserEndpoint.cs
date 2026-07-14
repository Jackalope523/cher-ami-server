using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Exceptions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Users
{
    public class BlockUserEndpoint(ApplicationDbContext ctx) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Post("/users/{id}/block");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            long targetId = Route<long>("id");

            if (userId == targetId)
                throw new NoPermissionException($"A user can not block themselves.");

            if (await ctx.Blocks.AnyAsync(x => x.BlockerId == userId && x.BlockedId == targetId))
                throw new ConflictException($"A user can not block another user multiple times.");

            Block block = new()
            {
                BlockerId = userId,
                BlockedId = targetId,
                BlockDate = DateTimeOffset.UtcNow
            };

            ctx.Blocks.Add(block);
            await ctx.SaveChangesAsync(cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
