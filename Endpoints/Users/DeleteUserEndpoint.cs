using CherAmiAPI.Interfaces;
using CherAmiAPI.Contexts;
using CherAmiAPI.Shared.Requests;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Users
{
    public class DeleteUserEndpoint(ApplicationDbContext ctx, IImageService imageService) : EndpointWithoutRequest
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
                string avatarUrl = await ctx.Users.Where(x => x.Id == userId).Select(x => x.AvatarPath).SingleAsync(cancellationToken: cancellationToken);
                List<string> recipientAvatars = await ctx.Recipients.Where(x => x.ManagerId == userId).Select(x => x.AvatarPath).ToListAsync(cancellationToken: cancellationToken);
                List<string> postImages = await ctx.Posts.Where(x => x.AuthorId == userId).Select(x => x.ImagePath).ToListAsync(cancellationToken: cancellationToken);

                List<string> pathsToDelete = [..recipientAvatars, ..postImages];
                if (avatarUrl != null)
                {
                    pathsToDelete.Add(avatarUrl);
                } 
                
                foreach (string path in pathsToDelete)
                {
                    await imageService.DeleteImageAsync(path);
                }

                await ctx.Reports.Where(x => x.FilingUserId == userId).ExecuteDeleteAsync(cancellationToken);
                await ctx.UserReports.Where(x => x.ReportedUserId == userId).ExecuteDeleteAsync(cancellationToken);
                await ctx.Posts.Where(x => x.AuthorId == userId).ExecuteDeleteAsync(cancellationToken);
                await ctx.Recipients.Where(x => x.ManagerId == userId).ExecuteDeleteAsync(cancellationToken);
                await ctx.Users.Where(x => x.Id == userId).ExecuteDeleteAsync(cancellationToken);

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