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
            Post("/ping");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            try
            {
                await ctx.Database.ExecuteSqlRawAsync("SELECT 1");
                await Send.NoContentAsync(cancellationToken);
                Log.Error("Ping successful.");
            }
            catch (Exception)
            {
               await Send.AcceptedAtAsync<PingEndpoint>(cancellation: cancellationToken);
                Log.Error("Ping accepted.");
            }
        }
    }
}
