using CherAmiAPI.Endpoints.Users;
using CherAmiAPI.Interfaces;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Services
{
    public class OneSignalService(HttpClient httpClient, IConfiguration config, IKeyService keyService)
    {
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

        public async Task AddTagAsync(Guid externalId, string key, string value, CancellationToken cancellationToken = default)
        {
            var payload = new
            {
                properties = new
                {
                    tags = new Dictionary<string, string> { { key, value } }
                }
            };

            HttpResponseMessage response = await httpClient.PatchAsJsonAsync($"users/by/external_id/{externalId}", payload, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveTagAsync(Guid externalId, string key, CancellationToken cancellationToken = default)
        {
            var payload = new
            {
                properties = new
                {
                    tags = new Dictionary<string, string?> { { key, null } }
                }
            };

            HttpResponseMessage response = await httpClient.PatchAsJsonAsync($"users/by/external_id/{externalId}", payload, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}
