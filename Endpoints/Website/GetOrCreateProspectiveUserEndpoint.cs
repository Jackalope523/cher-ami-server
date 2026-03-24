using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Exceptions;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Services;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Website
{
    public class GetOrCreateProspectiveUserRequest
    {
        public string Email { get; set; }
    }

    public class GetOrCreateProspectiveUserRequestValidator : Validator<GetOrCreateProspectiveUserRequest>
    {
        public GetOrCreateProspectiveUserRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be valid.");
        }
    }

    public class GetOrCreateProspectiveUserResponse
    {
        public Guid ExternalId { get; set; }
        public string OneSignalId { get; set; }
    }

    public class GetOrCreateProspectiveUserEndpoint(ApplicationDbContext ctx, IKeyService keyService, OneSignalService oneSignalService, UserManager<User> userManager) : Endpoint<GetOrCreateProspectiveUserRequest, GetOrCreateProspectiveUserResponse>
    {
        public override void Configure()
        {
            Post("/website/user");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetOrCreateProspectiveUserRequest request, CancellationToken cancellationToken)
        {
            string apiKey = HttpContext.Request.Headers["Authorization"].ToString()["key ".Length..];
            if (apiKey != await keyService.GetSecretAsync("Cher-Ami-API-Key"))
            {
                await Send.ForbiddenAsync(cancellationToken);
                return;
            }

            var user = await ctx.Users
                       .Where(u => u.Email == request.Email)
                       .Select(u => new { u.ExternalId, u.OneSignalId })
                       .FirstOrDefaultAsync(cancellationToken);

            GetOrCreateProspectiveUserResponse response;
            if (user != null)
            {
                response = new()
                {
                    ExternalId = user.ExternalId,
                    OneSignalId = user.OneSignalId
                };
            }
            else
            {
                User newUser = new()
                {
                    UserName = request.Email,
                    Email = request.Email,
                    ExternalId = Guid.NewGuid(),
                    AccountStatus = UserAccountStatus.Prospective,
                };

                newUser.OneSignalId = await oneSignalService.CreateUserAsync(newUser.ExternalId, newUser.Email, cancellationToken);
                await oneSignalService.AddTagAsync(newUser.ExternalId, "email_reminders", "1", cancellationToken);
                await oneSignalService.AddTagAsync(newUser.ExternalId, "email_marketing", "1", cancellationToken);

                var result = await userManager.CreateAsync(newUser);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        Log.Error("Error creating user: {Error}", error.Description);
                    }
                }

                response = new()
                {
                    ExternalId = newUser.ExternalId,
                    OneSignalId = newUser.OneSignalId
                };
            }

            await Send.OkAsync(response, cancellationToken);
        }
    }
}
