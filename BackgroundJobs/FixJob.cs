using CherAmiAPI.Contexts;
using CherAmiAPI.Endpoints.Users;
using CherAmiAPI.Entities;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using Stripe;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CherAmiAPI.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public class FixJob(IServiceProvider _serviceProvider) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            ApplicationDbContext ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            HttpClient httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
            IKeyService keyService = scope.ServiceProvider.GetRequiredService<IKeyService>();
            CustomerService customerService = scope.ServiceProvider.GetRequiredService<CustomerService>();

            httpClient.DefaultRequestHeaders.Add("Authorization", $"key {await keyService.GetSecretAsync("OneSignal-API-Key")}");

            Log.Error("Starting repairs.");
            List<User> users = await ctx.Users.ToListAsync();

            foreach (User user in users)
            {
                Log.Error("Fixing " + user.Email);

                //user.ExternalId = Guid.NewGuid();
                //user.StripeCustomerId = null;

                //if (user.OneSignalId == null)
                //{
                //    Log.Error("Has no OneSignal Id");
                //    var body = new
                //    {
                //        identity = new { external_id = user.ExternalId },
                //        subscriptions = new[] { new { type = "Email", token = user.Email } },
                //    };

                //    using JsonContent jsonBody = JsonContent.Create(body);
                //    string app_id = await keyService.GetSecretAsync("OneSignal-App-Id");

                //    HttpResponseMessage response = await httpClient.PostAsync($"https://api.onesignal.com/apps/{app_id}/users", jsonBody);

                //    if (!response.IsSuccessStatusCode)
                //    {
                //        Log.Error("Failed to create OneSignal user for " + user.Email + ": " + await response.Content.ReadAsStringAsync());
                //    }
                //    response.EnsureSuccessStatusCode();

                //    OneSignalCreateUserResponse content = await response.Content.ReadFromJsonAsync<OneSignalCreateUserResponse>();
                //    user.OneSignalId = content.Identity.OneSignalId;

                //    await ctx.SaveChangesAsync();
                //}

                // JACKALOPE: Temporary fix for users with invalid OneSignal IDs.
                //if (user.JoinDate < new DateTimeOffset(2026, 1, 20, 0, 0, 0, TimeSpan.Zero))
                //{
                //    var body = new
                //    {
                //        identity = new { external_id = user.ExternalId.ToString() },
                //    };

                //    using JsonContent jsonBody = JsonContent.Create(body);
                //    string app_id = await keyService.GetSecretAsync("OneSignal-App-Id");

                //    HttpResponseMessage response = await httpClient.PatchAsync($"https://api.onesignal.com/apps/{app_id}/users/by/onesignal_id/{user.OneSignalId}/identity", jsonBody);
                //    response.EnsureSuccessStatusCode();
                //}

                if (user.StripeCustomerId == null)
                {
                    var options = new CustomerCreateOptions
                    {
                        Name = $"{user.FirstName} {user.LastName}",
                        Email = user.Email,
                    };

                    Customer customer = await customerService.CreateAsync(options);
                    user.StripeCustomerId = customer.Id;
                }
            }
            await ctx.SaveChangesAsync();
            Log.Error("Done repairs.");
        }
    }
}
