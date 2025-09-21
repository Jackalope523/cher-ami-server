using CrazyLizard.Interfaces;
using FastEndpoints;
using FluentValidation;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Notifications
{
    public class UpdateNotificationPreferencesRequest
    {
        public bool? IssuePosts { get; set; }
        public bool? IssueReminders { get; set; }
    }

    public class UpdateNotificationPreferencesRequestValidator : Validator<UpdateNotificationPreferencesRequest>
    {
        public UpdateNotificationPreferencesRequestValidator()
        {
            RuleFor(x => x)
                .Must(x => x.IssuePosts.HasValue || x.IssueReminders.HasValue)
                .WithMessage("At least one of IssuePosts or IssueReminders must be provided.");
        }
    }


    public class UpdateNotificationPreferencesEndpoint(INotificationStorageService notificationService) : Endpoint<UpdateNotificationPreferencesRequest>
    {
        public override void Configure()
        {
            Post("/notifications");
        }

        public override async Task HandleAsync(UpdateNotificationPreferencesRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await notificationService.UpdateNotificationPreferencesAsync(userId, request.IssuePosts, request.IssueReminders);
            await Send.NoContentAsync(cancellationToken);
        }
    }
}