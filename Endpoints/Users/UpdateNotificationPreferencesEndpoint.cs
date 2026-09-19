using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Services;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Users
{
    public class NotificationPreferencesRequest
    {
        public bool PushNewPosts { get; set; }
        public bool PushIssueReminders { get; set; }
        public bool PushNewMembers { get; set; }
        public bool EmailIssueReminders { get; set; }
        public bool EmailMarketing { get; set; }
    }

    public class UpdateNotificationPreferencesEndpoint(ApplicationDbContext ctx, NotificationService notificationService) : Endpoint<NotificationPreferencesRequest>
    {
        public override void Configure()
        {
            Put("/user/notifications");
        }

        public override async Task HandleAsync(NotificationPreferencesRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await ctx.Users
                .Where(x => x.Id == userId)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(u => u.PushNewPosts, request.PushNewPosts)
                    .SetProperty(u => u.PushIssueReminders, request.PushIssueReminders)
                    .SetProperty(u => u.PushNewMembers, request.PushNewMembers)
                    .SetProperty(u => u.EmailIssueReminders, request.EmailIssueReminders)
                    .SetProperty(u => u.EmailMarketing, request.EmailMarketing), cancellationToken);

            await notificationService.SyncEmailPreferencesAsync(userId, cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
