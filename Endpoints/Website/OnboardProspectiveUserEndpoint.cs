using CherAmiAPI.Interfaces;
using CherAmiAPI.Services;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Website
{
    public class OnboardProspectiveUserRequest
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> FriendEmails { get; set; } = [];
        public string RecipientName { get; set; }
        public string Caption { get; set; }
        public IFormFile Image { get; set; }
    }

    public class OnboardProspectiveUserRequestValidator : Validator<OnboardProspectiveUserRequest>
    {
        public OnboardProspectiveUserRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be valid.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

            RuleFor(x => x.RecipientName)
                .MaximumLength(60).WithMessage("Recipient name cannot exceed 60 characters.");

            RuleFor(x => x.Caption)
                .MaximumLength(200).WithMessage("Caption cannot exceed 200 characters.");

            RuleForEach(x => x.FriendEmails)
                .EmailAddress().WithMessage("All friend emails must be valid.");
        }
    }

    public class OnboardProspectiveUserResponse
    {
        public Guid ExternalId { get; set; }
    }

    public class OnboardProspectiveUserEndpoint(IKeyService keyService, OnboardingService onboardingService) : Endpoint<OnboardProspectiveUserRequest, OnboardProspectiveUserResponse>
    {
        public override void Configure()
        {
            Post("/website/onboarding");
            AllowAnonymous();
            AllowFileUploads();
        }

        public override async Task HandleAsync(OnboardProspectiveUserRequest request, CancellationToken cancellationToken)
        {
            string apiKey = HttpContext.Request.Headers["Authorization"].ToString()["key ".Length..];
            if (apiKey != await keyService.GetSecretAsync("Cher-Ami-API-Key"))
            {
                await Send.ForbiddenAsync(cancellationToken);
                return;
            }

            Guid? externalId = await onboardingService.OnboardProspectiveUserAsync(
                request.Email,
                request.FirstName,
                request.LastName,
                request.FriendEmails,
                request.RecipientName,
                request.Caption,
                request.Image,
                cancellationToken);

            if (externalId == null)
            {
                await Send.NoContentAsync(cancellationToken);
                return;
            }

            await Send.OkAsync(new OnboardProspectiveUserResponse { ExternalId = externalId.Value }, cancellationToken);
        }
    }
}
