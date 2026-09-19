using CherAmiAPI.Contexts;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Services;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Website
{
    public class UpdateEmailPreferencesRequest
    {
        public Guid ExternalId { get; set; }
        public bool Reminders { get; set; }
        public bool Marketing { get; set; }
    }

    public class UpdateEmailPreferencesEndpoint(ApplicationDbContext ctx, IKeyService keyService, NotificationService notificationService) : Endpoint<UpdateEmailPreferencesRequest>
    {
        public override void Configure()
        {
            Put("/website/preferences/{ExternalId}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(UpdateEmailPreferencesRequest request, CancellationToken cancellationToken)
        {
            if (!await WebsiteApiKey.IsValidAsync(HttpContext, keyService, cancellationToken))
            {
                await Send.ForbiddenAsync(cancellationToken);
                return;
            }

            long userId = await ctx.Users
                .Where(x => x.ExternalId == request.ExternalId)
                .Select(x => x.Id)
                .SingleOrDefaultAsync(cancellationToken);

            if (userId == default)
            {
                await Send.NotFoundAsync(cancellationToken);
                return;
            }

            await ctx.Users
                .Where(x => x.Id == userId)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(u => u.EmailIssueReminders, request.Reminders)
                    .SetProperty(u => u.EmailMarketing, request.Marketing), cancellationToken);

            await notificationService.SyncEmailPreferencesAsync(userId, cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
