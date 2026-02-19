using CherAmiAPI.Endpoints.Users;
using CherAmiAPI.Entities;
using CherAmiAPI.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Services
{
    public class OneSignalService(IConfiguration config, HttpClient httpClient, IKeyService keyService, CancellationToken cancellationToken = default)
    {
        public async Task<string> CreateUserAsync(Guid externalId, string email)
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"key {await keyService.GetSecretAsync("OneSignal-API-Key")}");

            var oneSignalBody = new
            {
                identity = new { external_id = externalId },
                subscriptions = new[] { new { type = "Email", token = email, enabled = true, notification_types = 1 } },
            };

            using JsonContent oneSignalJsonBody = JsonContent.Create(oneSignalBody);

            HttpResponseMessage oneSignalResponse = await httpClient.PostAsync($"https://api.onesignal.com/apps/{config["ONESIGNAL_APP_ID"]}/users", oneSignalJsonBody, cancellationToken);
            oneSignalResponse.EnsureSuccessStatusCode();

            OneSignalCreateUserResponse oneSignalContent = await oneSignalResponse.Content.ReadFromJsonAsync<OneSignalCreateUserResponse>(cancellationToken: cancellationToken);
            return oneSignalContent.Identity.OneSignalId;
        }
    }
}
