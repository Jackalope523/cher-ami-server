using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Shared.Mappers;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Users
{
    public class GetSelfEndpoint(ApplicationDbContext ctx) : EndpointWithoutRequest<UserDTO, UserResponseMapper>
    {
        public override void Configure()
        {
            Get("/user");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            User user = await ctx.Users.Where(x => x.Id == userId).Include(x => x.Recipients).SingleAsync();
            await Send.OkAsync(Map.FromEntity(user), cancellationToken);
        }
    }
}