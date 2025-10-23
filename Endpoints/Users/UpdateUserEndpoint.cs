using CherAmiAPI.Interfaces;
using CrazyLizard.Contexts;
using CrazyLizard.Entities;
using CrazyLizard.Exceptions;
using CrazyLizard.Shared.Mappers;
using CrazyLizard.Shared.Requests;
using CrazyLizard.Shared.Responses;
using CrazyLizard.Shared.SharedMappers;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Users
{
    public class UpdateUserRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public IFormFile Avatar { get; set; }
        public string InviteCode { get; set; }

    }

    public class UpdateUserRequestValidator : Validator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
  
        }
    }

    public class UpdateUserEndpoint(ApplicationDbContext ctx, IImageService imageService) : Endpoint<UpdateUserRequest, UserDTO, UserResponseMapper>
    {
        public override void Configure()
        {
            Put("/user");
            AllowFileUploads();
        }

        public override async Task HandleAsync(UpdateUserRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await using var transaction = await ctx.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                User user = await ctx.Users.Where(x => x.Id == userId).SingleAsync(cancellationToken: cancellationToken);

                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.DateOfBirth = request.DateOfBirth;
                user.JoinDate = DateTimeOffset.UtcNow;

                using var stream = new MemoryStream();
                await request.Avatar.CopyToAsync(stream, cancellationToken);

                string path = $"users/{user.Id}/avatar.jpg";

                user.AvatarPath = path;
                user.AvatarTimestamp = DateTimeOffset.UtcNow;

                await imageService.UploadImageAsync(path, stream);

                long circleId = await ctx.Circles.Where(x => x.CircleCode == request.InviteCode).Select(x => x.Id).SingleAsync(cancellationToken: cancellationToken);
                user.CircleId = circleId;

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