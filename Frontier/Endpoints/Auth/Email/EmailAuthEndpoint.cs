using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using CrazyLizard.Entities;
using System;
using Serilog;

namespace CrazyLizard.Endpoints.Auth.Email
{
    public class EmailAuthRequest
    {
        public string Email { get; set; }
    }

    public class EmailAuthRequestValidator : Validator<EmailAuthRequest>
    {
        public EmailAuthRequestValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email must be valid.")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));
        }
    }

    public class EmailAuthEndpoint(UserManager<User> userManager) : Endpoint<EmailAuthRequest>
    {
        public override void Configure()
        {
            Post("/auth/email");
            AllowAnonymous();
        }

        public override async Task HandleAsync(EmailAuthRequest request, CancellationToken cancellationToken)
        {
            User user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
         
                User toCreate = new()
                {
                    UserName = request.Email,
                    Email = request.Email,
                };

                await userManager.CreateAsync(toCreate);

                Log.Error("CREATING USER: " + request.Email);
                User user2 = await userManager.FindByEmailAsync(request.Email);
                Log.Error("Found: " + user2.Email);
            }

            // JACKALOPE: Send them an email.

            await Send.NoContentAsync(cancellationToken);
        }
    }
}