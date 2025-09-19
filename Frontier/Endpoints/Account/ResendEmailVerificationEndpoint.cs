using Core.Boundaries;
using CrazyLizard.Entities;
using CrazyLizard.Exceptions;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class EmailRequest
    {
        public string Email { get; set; }
    }

    public class EmailRequestValidator : Validator<EmailRequest>
    {
        public EmailRequestValidator()
        {
            RuleFor(x => x.Email).
            NotEmpty().WithMessage("Email is required.");
        }
    }

    public class ResendEmailVerificstion(UserManager<User> userManager, IEmailService emailService) : Endpoint<EmailRequest>
    {
        public override void Configure()
        {
            Post("/account/email");
            AllowAnonymous();
        }

        public override async Task HandleAsync(EmailRequest request, CancellationToken cancellationToken)
        {
            User user = await userManager.FindByEmailAsync(request.Email);

            if (user is null)
                throw new NotFoundException($"Could not find User with email {request.Email}.");

            if (!user.IsEmailConfirmed)
            {
                string token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                // Replace this with your own method to generate a URL
                string confirmationLink = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/account/email?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";

                await emailService.SendEmailAsync(
                    user.Email,
                    "Verify your CANARY email.",
                    $"Verify your CANARY email.\n\n{confirmationLink}");
            }

            await Send.NoContentAsync(cancellationToken);
        }
    }
}