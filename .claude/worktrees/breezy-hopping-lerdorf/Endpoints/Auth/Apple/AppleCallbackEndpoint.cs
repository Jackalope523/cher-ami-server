using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using Serilog;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Auth.Apple
{
    public class Name
    {
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }
    }

    public class AppleUser
    {
        [JsonPropertyName("name")]
        public Name Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }
    }


    public class AppleCallbackRequest
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("user")]
        public AppleUser User { get; set; }
    }

    public class AppleCallbackRequestValidator : Validator<AppleCallbackRequest>
    {
        public AppleCallbackRequestValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required.");
        }
    }

    public class AppleCallbackEndpoint(ApplicationDbContext ctx, UserManager<User> userManager) : Endpoint<AppleCallbackRequest>
    {
        public override void Configure()
        {
            Post("/auth/apple/callback");
            AllowFormData(urlEncoded: true);
            AllowAnonymous();
        }

        public override async Task HandleAsync(AppleCallbackRequest request, CancellationToken cancellationToken)
        {
            if (request.User != null && !await ctx.Users.AnyAsync(x => x.Email == request.User.Email && x.FirstName != null && x.LastName != null, cancellationToken: cancellationToken))
            {
                User user = new()
                {
                    UserName = request.User.Email,
                    Email = request.User.Email,
                    FirstName = request.User.Name.FirstName,
                    LastName = request.User.Name.LastName,
                };

                await userManager.CreateAsync(user);
            }


            Dictionary<string, string> queryParams = new()
            {
                ["code"] = request.Code,
                ["state"] = request.State,
            };

            string redirectUrl = QueryHelpers.AddQueryString("cherami://", queryParams);

            await Send.RedirectAsync(redirectUrl, true, true);
        }
    }
}