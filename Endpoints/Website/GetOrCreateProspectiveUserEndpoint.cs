using CherAmiAPI.Interfaces;
using CherAmiAPI.Services;
using FastEndpoints;
using FluentValidation;
using System;
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

    public class GetOrCreateProspectiveUserEndpoint(IKeyService keyService, OnboardingService onboardingService) : Endpoint<GetOrCreateProspectiveUserRequest, GetOrCreateProspectiveUserResponse>
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

            (Guid externalId, string oneSignalId) = await onboardingService.GetOrCreateProspectiveUserAsync(request.Email, cancellationToken);

            GetOrCreateProspectiveUserResponse response = new()
            {
                ExternalId = externalId,
                OneSignalId = oneSignalId
            };

            await Send.OkAsync(response, cancellationToken);
        }
    }
}
