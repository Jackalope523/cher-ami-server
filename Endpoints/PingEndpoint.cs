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
            const int maxRetries = 3;
            const int delaySeconds = 10;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    await ctx.Database.ExecuteSqlRawAsync("SELECT 1");
                    await Send.NoContentAsync(cancellationToken);
                    return;
                }
                catch (Exception)
                {
                    if (i == maxRetries - 1)
                    {
                        throw;
                    }
                     
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                }
            }
        }
    }
}
