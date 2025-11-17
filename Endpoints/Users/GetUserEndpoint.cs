using CherAmiAPI.Exceptions;
using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Shared.Mappers;
using CherAmiAPI.Shared.Requests;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Users
{
    public class GetUserEndpoint(ApplicationDbContext ctx) : Endpoint<IdRequest, UserDTO, UserResponseMapper>
    {
        public override void Configure()
        {
            Get("/users/{id}");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {

            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (userId != request.Id)
            {
                int count = await ctx.Users.Where(x => x.Id == userId || x.Id == request.Id).Select(x => x.CircleId).Distinct().CountAsync(cancellationToken: cancellationToken);

                if (count > 1)
                    throw new NoAccessException($"User {userId} can not access this user {request.Id}.");
            }

            Console.WriteLine("HIT");
            User user = await ctx.Users.Where(x => x.Id == request.Id).Include(x => x.Recipients).SingleAsync(cancellationToken: cancellationToken);
            await Send.OkAsync(Map.FromEntity(user), cancellationToken);
        }
    }
}