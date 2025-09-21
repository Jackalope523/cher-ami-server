using CrazyLizard.Interfaces;
using FastEndpoints;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Notifications
{
    public record NotificationPreferencesResponse
    {
        public Guid Notificationid { get; init; }
        public bool IssuePosts { get; init; }
        public bool IssueReminders { get; init; }
    }

    public class NotificationPreferencesMapper : ResponseMapper<NotificationPreferencesResponse, CoreNotificationProfile>
    {
        public override NotificationPreferencesResponse FromEntity(CoreNotificationProfile notificationProfile) => new()
        {
            Notificationid = notificationProfile.NotificationId,
            IssuePosts = notificationProfile.IssuePosts,
            IssueReminders = notificationProfile.IssueReminders,
        };
    }

    public class GetNotificationPreferencesEndpoint(INotificationStorageService notificationService) : EndpointWithoutRequest<NotificationPreferencesResponse, NotificationPreferencesMapper>
    {
        public override void Configure()
        {
            Get("/notifications");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            CoreNotificationProfile corePreferences = await notificationService.GetNotificationPreferencesAsync(userId);

            await Send.OkAsync(Map.FromEntity(corePreferences), cancellationToken);

        }
    }
}