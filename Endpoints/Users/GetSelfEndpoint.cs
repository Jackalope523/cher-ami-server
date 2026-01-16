using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Shared.Mappers;
using CherAmiAPI.Shared.Responses;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Stripe;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Users
{
    public class GetSelfEndpoint(ApplicationDbContext ctx, HttpClient httpClient, IKeyService keyService, CustomerService customerService) : EndpointWithoutRequest<UserDTO, UserResponseMapper>
    {
        public override void Configure()
        {
            Get("/user");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            User user = await ctx.Users.Where(x => x.Id == userId).Include(x => x.Recipients).SingleAsync(cancellationToken: cancellationToken);

            if (user.OneSignalId == null) 
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"key {await keyService.GetSecretAsync("OneSignal-API-Key")}");

                var body = new
                {
                    identity = new { external_id = user.Id.ToString() },
                    subscriptions = new[] { new { type = "Email", token = user.Email } },
                };

                using JsonContent jsonBody = JsonContent.Create(body);
                string app_id = await keyService.GetSecretAsync("OneSignal-App-Id");

                HttpResponseMessage response = await httpClient.PostAsync($"https://api.onesignal.com/apps/{app_id}/users", jsonBody, cancellationToken);
                response.EnsureSuccessStatusCode();

                OneSignalCreateUserResponse content = await response.Content.ReadFromJsonAsync<OneSignalCreateUserResponse>(cancellationToken: cancellationToken);
                user.OneSignalId = content.Identity.OneSignalId;

                await ctx.SaveChangesAsync(cancellationToken);
            }

            if (user.StripeCustomerId == null)
            {
                var options = new CustomerCreateOptions
                {
                    Name = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                };

                Customer customer = await customerService.CreateAsync(options, cancellationToken: cancellationToken);
                user.StripeCustomerId = customer.Id;

                await ctx.SaveChangesAsync(cancellationToken);
            }

            await Send.OkAsync(Map.FromEntity(user), cancellationToken);
        }
    }
}