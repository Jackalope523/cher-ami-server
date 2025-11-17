using CherAmiAPI.Exceptions;
using CherAmiAPI.Contexts;
using CherAmiAPI.Entities.Reports;
using CherAmiAPI.Shared.Requests;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Issues
{
    public class ReportPostEndpoint(ApplicationDbContext ctx) : Endpoint<IdRequest>
    {
        public override void Configure()
        {
            Post("/posts/{id}/report");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (await ctx.Posts.AnyAsync(x => x.Id == request.Id && x.AuthorId == userId, cancellationToken: cancellationToken)) 
                throw new NoPermissionException("You can't report your own posts.");

            PostReport report = new()
            {
                FilingUserId = userId,
                FilingDate = DateTimeOffset.UtcNow,
                PostId = request.Id,
                Type = PostReportType.Other,
            };

            ctx.PostReports.Add(report);
            await ctx.SaveChangesAsync(cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}