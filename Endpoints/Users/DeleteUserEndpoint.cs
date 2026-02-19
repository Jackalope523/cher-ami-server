using CherAmiAPI.Contexts;
using CherAmiAPI.Interfaces;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Users
{
    public class DeleteUserEndpoint(IConfiguration config, ApplicationDbContext ctx, IImageService imageService, HttpClient httpClient, IKeyService keyService, CustomerService customerService) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Delete("/user");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            await Send.NoContentAsync(cancellationToken);

            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await using var transaction = await ctx.Database.BeginTransactionAsync(cancellationToken);
            var user = await ctx.Users.Where(x => x.Id == userId).Select(x => new { x.AvatarPath, x.StripeCustomerId, x.ExternalId }).SingleAsync(cancellationToken: cancellationToken);

            string app_id = config["ONESIGNAL_APP_ID"];
            string api_key = await keyService.GetSecretAsync("OneSignal-API-Key");

            List<string> recipientAvatars = await ctx.Recipients.Where(x => x.ManagerId == userId).Select(x => x.AvatarPath).ToListAsync(cancellationToken: cancellationToken);
            List<string> postImages = await ctx.Posts.Where(x => x.AuthorId == userId).Select(x => x.LowResolutionImagePath).ToListAsync(cancellationToken: cancellationToken);

            List<string> toDelete = [.. recipientAvatars, .. postImages];

            if (user.AvatarPath != null)
            {
                toDelete.Add(user.AvatarPath);
            }

            try
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"key {api_key}");
                HttpResponseMessage response = await httpClient.DeleteAsync($"https://api.onesignal.com/apps/{app_id}/users/by/external_id/{user.ExternalId}", cancellationToken);
                response.EnsureSuccessStatusCode();

                await customerService.DeleteAsync(user.StripeCustomerId, cancellationToken: cancellationToken);

                await ctx.Reports.Where(x => x.FilingUserId == userId).ExecuteDeleteAsync(cancellationToken);
                await ctx.UserReports.Where(x => x.ReportedUserId == userId).ExecuteDeleteAsync(cancellationToken);
                await ctx.Posts.Where(x => x.AuthorId == userId).ExecuteDeleteAsync(cancellationToken);
                await ctx.Recipients.Where(x => x.ManagerId == userId).ExecuteDeleteAsync(cancellationToken);
                await ctx.Users.Where(x => x.Id == userId).ExecuteDeleteAsync(cancellationToken);

                await imageService.DeleteImagesAsync(toDelete);

                await ctx.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}