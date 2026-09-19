using CherAmiAPI.Contexts;
using CherAmiAPI.Interfaces;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Website
{
    public class EmailPreferencesRequest
    {
        public Guid ExternalId { get; set; }
    }

    public class EmailPreferencesResponse
    {
        /// <summary>Enough for the page to say whose preferences these are, without handing out the address.</summary>
        public string MaskedEmail { get; init; }
        public bool Reminders { get; init; }
        public bool Marketing { get; init; }
    }

    public class GetEmailPreferencesEndpoint(ApplicationDbContext ctx, IKeyService keyService) : Endpoint<EmailPreferencesRequest, EmailPreferencesResponse>
    {
        public override void Configure()
        {
            Get("/website/preferences/{ExternalId}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(EmailPreferencesRequest request, CancellationToken cancellationToken)
        {
            if (!await WebsiteApiKey.IsValidAsync(HttpContext, keyService, cancellationToken))
            {
                await Send.ForbiddenAsync(cancellationToken);
                return;
            }

            var user = await ctx.Users
                .AsNoTracking()
                .Where(x => x.ExternalId == request.ExternalId)
                .Select(x => new { x.Email, x.EmailIssueReminders, x.EmailMarketing })
                .SingleOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                await Send.NotFoundAsync(cancellationToken);
                return;
            }

            await Send.OkAsync(new EmailPreferencesResponse
            {
                MaskedEmail = Mask(user.Email),
                Reminders = user.EmailIssueReminders,
                Marketing = user.EmailMarketing,
            }, cancellationToken);
        }

        private static string Mask(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "";

            int at = email.IndexOf('@');

            if (at <= 0) return "•••";

            return $"{email[0]}•••{email[at..]}";
        }
    }
}
