using CherAmiAPI.Contexts;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints
{
    public class PingEndpoint(ApplicationDbContext ctx) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/ping");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            try
            {
                await ctx.Users.AnyAsync();
                await Send.NoContentAsync(cancellationToken);
            }
            catch (Exception)
            {
               await Send.AcceptedAtAsync<PingEndpoint>(cancellation: cancellationToken);
            }
        }
    }
}
