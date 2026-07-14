using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Shared.Mappers;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using static CherAmiAPI.Entities.Notification;

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
            User user = await ctx.Users.Where(x => x.Id == userId).Include(x => x.Recipients).SingleAsync(cancellationToken: cancellationToken);

            await Send.OkAsync(Map.FromEntity(user), cancellationToken);
        }
    }
}