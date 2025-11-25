using CherAmiAPI.Contexts;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Users
{
    public class GetBlockedEndpoint(ApplicationDbContext ctx) : EndpointWithoutRequest<List<UserItem>>
    {
        public override void Configure()
        {
            Get("/users/blocked");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            List<UserItem> response = await ctx.Blocks
                                      .Where(x => x.BlockerId == userId)
                                      .Select(x => new UserItem() 
                                      { 
                                          Id = x.Blocked.Id, 
                                          FirstName = x.Blocked.FirstName,
                                          LastName = x.Blocked.LastName,
                                          AvatarPath = x.Blocked.AvatarPath,
                                          AvatarTimestamp = x.Blocked.AvatarTimestamp,

                                      })
                                      .ToListAsync();

            for(int i = 0; i < response.Count; i++)
            {
                response[i] = response[i] with { AvatarPath = response[i].AvatarPath == null ? null : $"/users/{response[i].Id}/avatar" };
            }

            await Send.OkAsync(response, cancellationToken);
        }
    }
}