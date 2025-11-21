using CherAmiAPI.Interfaces;
using CherAmiAPI.Contexts;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using Stripe;

namespace CherAmiAPI.Endpoints.Users
{
    public class DeleteUserEndpoint(ApplicationDbContext ctx, IImageService imageService, HttpClient httpClient, IKeyService keyService, CustomerService customerService) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Delete("/user");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await using var transaction = await ctx.Database.BeginTransactionAsync(cancellationToken);

            try
            { 
                var user = await ctx.Users.Where(x => x.Id == userId).Select(x => new {x.AvatarPath, x.StripeCustomerId }).SingleAsync(cancellationToken: cancellationToken);
                List<string> recipientAvatars = await ctx.Recipients.Where(x => x.ManagerId == userId).Select(x => x.AvatarPath).ToListAsync(cancellationToken: cancellationToken);
                List<string> postImages = await ctx.Posts.Where(x => x.AuthorId == userId).Select(x => x.ImagePath).ToListAsync(cancellationToken: cancellationToken);

                await imageService.DeleteImagesAsync([user.AvatarPath, .. recipientAvatars, .. postImages]);

                await ctx.Reports.Where(x => x.FilingUserId == userId).ExecuteDeleteAsync(cancellationToken);
                await ctx.UserReports.Where(x => x.ReportedUserId == userId).ExecuteDeleteAsync(cancellationToken);
                await ctx.Posts.Where(x => x.AuthorId == userId).ExecuteDeleteAsync(cancellationToken);
                await ctx.Recipients.Where(x => x.ManagerId == userId).ExecuteDeleteAsync(cancellationToken);
                await ctx.Users.Where(x => x.Id == userId).ExecuteDeleteAsync(cancellationToken);

                string app_id = await keyService.GetSecretAsync("OneSignal-App-Id");
                string api_key = await keyService.GetSecretAsync("OneSignal-API-Key");

                httpClient.DefaultRequestHeaders.Add("Authorization", $"key {api_key}");
                await httpClient.DeleteAsync($"https://api.onesignal.com/apps/{app_id}/users/by/external_id/{userId}", cancellationToken);

                await customerService.DeleteAsync(user.StripeCustomerId, cancellationToken: cancellationToken);

                await ctx.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await Send.NoContentAsync(cancellationToken);
            }
            catch (Exception)
            {
               await transaction.RollbackAsync(cancellationToken);
               throw;
            }
        }
    }
}