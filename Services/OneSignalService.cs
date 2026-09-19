using CherAmiAPI.Endpoints.Users;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Services
{
    public class OneSignalService(HttpClient httpClient, IConfiguration config)
    {
        private const string NotificationsUrl = "https://api.onesignal.com/notifications";

        public async Task<string> CreateUserAsync(Guid externalId, string email, CancellationToken cancellationToken = default)
        {
            var oneSignalBody = new
            {
                identity = new { external_id = externalId },
                subscriptions = new[] { new { type = "Email", token = email, enabled = true, notification_types = 1 } },
            };

            HttpResponseMessage oneSignalResponse = await httpClient.PostAsJsonAsync($"users", oneSignalBody, cancellationToken);
            oneSignalResponse.EnsureSuccessStatusCode();

            OneSignalCreateUserResponse oneSignalContent = await oneSignalResponse.Content.ReadFromJsonAsync<OneSignalCreateUserResponse>(cancellationToken: cancellationToken);
            return oneSignalContent.Identity.OneSignalId;
        }

        public async Task SetTagsAsync(Guid externalId, IDictionary<string, string> tags, CancellationToken cancellationToken = default)
        {
            if (tags.Count == 0) return;

            var payload = new { properties = new { tags } };

            HttpResponseMessage response = await httpClient.PatchAsJsonAsync($"users/by/external_id/{externalId}", payload, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public Task AddTagAsync(Guid externalId, string key, string value, CancellationToken cancellationToken = default)
            => SetTagsAsync(externalId, new Dictionary<string, string> { { key, value } }, cancellationToken);

        public Task RemoveTagAsync(Guid externalId, string key, CancellationToken cancellationToken = default)
            => SetTagsAsync(externalId, new Dictionary<string, string> { { key, "" } }, cancellationToken);

        public async Task TrackEventAsync(Guid externalId, string eventName, CancellationToken cancellationToken = default)
        {
            var payload = new
            {
                events = new[]
                {
                    new { name = eventName, external_id = externalId, timestamp = DateTimeOffset.UtcNow.ToString("O") }
                }
            };

            HttpResponseMessage response = await httpClient.PostAsJsonAsync("custom_events", payload, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        /// <param name="idempotencyKey">Deduplicates for 30 days, so a job that runs twice in a day doesn't send twice.</param>
        public async Task SendPushAsync(
            IReadOnlyCollection<Guid> externalIds,
            string heading,
            string content,
            IDictionary<string, string> data = null,
            Guid? idempotencyKey = null,
            CancellationToken cancellationToken = default)
        {
            if (externalIds.Count == 0) return;

            Dictionary<string, object> payload = new()
            {
                ["app_id"] = config["ONESIGNAL_APP_ID"],
                ["target_channel"] = "push",
                ["include_aliases"] = new Dictionary<string, string[]> { ["external_id"] = externalIds.Select(x => x.ToString()).ToArray() },
                ["headings"] = new Dictionary<string, string> { ["en"] = heading },
                ["contents"] = new Dictionary<string, string> { ["en"] = content },
            };

            if (data != null) payload["data"] = data;
            if (idempotencyKey.HasValue) payload["idempotency_key"] = idempotencyKey.Value.ToString();

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(NotificationsUrl, payload, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task SendTemplatedEmailAsync(
            string templateId,
            IReadOnlyCollection<string> emails,
            object customData = null,
            bool includeUnsubscribed = false,
            Guid? idempotencyKey = null,
            CancellationToken cancellationToken = default)
        {
            if (emails.Count == 0) return;

            Dictionary<string, object> payload = new()
            {
                ["app_id"] = config["ONESIGNAL_APP_ID"],
                ["template_id"] = templateId,
                ["email_to"] = emails,
                ["include_unsubscribed"] = includeUnsubscribed,
            };

            if (customData != null) payload["custom_data"] = customData;
            if (idempotencyKey.HasValue) payload["idempotency_key"] = idempotencyKey.Value.ToString();

            HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{NotificationsUrl}?c=email", payload, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public static Guid IdempotencyKeyFor(string value)
            => new(MD5.HashData(Encoding.UTF8.GetBytes(value)));
    }
}
