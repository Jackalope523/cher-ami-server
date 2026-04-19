using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CherAmiAPI.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public class AddEmailSubscriptionJob(IServiceProvider _serviceProvider) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            ApplicationDbContext ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            IKeyService keyService = scope.ServiceProvider.GetRequiredService<IKeyService>();
            IConfiguration config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            IHttpClientFactory httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

            string email = "ecote523@gmail.com";
            Log.Information("Starting AddEmailSubscriptionJob for {Email}", email);

            try
            {
                User user = await ctx.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    Log.Error("User with email {Email} not found.", email);
                    return;
                }

                if (user.ExternalId == default)
                {
                    Log.Error("User {Email} does not have an ExternalId.", email);
                    return;
                }

                string appId = config["ONESIGNAL_APP_ID"];
                string apiKey = await keyService.GetSecretAsync("OneSignal-API-Key");

                HttpClient httpClient = httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"key {apiKey}");

                var subscriptionPayload = new
                {
                    subscription = new
                    {
                        type = "Email",
                        token = email,
                        enabled = true,
                        notification_types = 1 // 1 is for "Subscribed"
                    }
                };

                string url = $"https://api.onesignal.com/apps/{appId}/users/by/external_id/{user.ExternalId}/subscriptions";
                
                HttpResponseMessage response = await httpClient.PostAsJsonAsync(url, subscriptionPayload);

                if (response.IsSuccessStatusCode)
                {
                    Log.Information("Successfully added email subscription for {Email} ({ExternalId})", email, user.ExternalId);
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Log.Error("Failed to add email subscription for {Email}. Status: {Status}, Error: {Error}", email, response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AddEmailSubscriptionJob failed unexpectedly for {Email}", email);
            }
        }
    }
}
