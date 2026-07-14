using CherAmiAPI.Exceptions;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Shared.Requests;
using FastEndpoints;
using Serilog;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Issues
{
    public class DeletePostEndpoint(ApplicationDbContext ctx, IImageService imageService) : Endpoint<IdRequest>
    {
        public override void Configure()
        {
            Delete("/posts/{id}");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Post post = await ctx.Posts.FindAsync(request.Id);

            if (post.AuthorId != userId) 
                throw new NoAccessException($"User {userId} is the not the author of post {post.Id}.");

            await using var transaction = await ctx.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await imageService.DeleteImageAsync(post.ImagePath);

                ctx.Posts.Remove(post);
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