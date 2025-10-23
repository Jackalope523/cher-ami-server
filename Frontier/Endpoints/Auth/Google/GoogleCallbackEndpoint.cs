using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Auth.Google
{
    public class GoogleCallbackRequest
    {
        public string Code { get; set; }
        public string State { get; set; }
    }

    public class GoogleCallbackRequestValidator : Validator<GoogleCallbackRequest>
    {
        public GoogleCallbackRequestValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required.");

            // JACKALOPE: Maybe use http sessions to verify the code properly. 
        }
    }

    public class GoogleCallbackEndpoint() : Endpoint<GoogleCallbackRequest>
    {
        public override void Configure()
        {
            Get("/auth/google/callback");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GoogleCallbackRequest request, CancellationToken cancellationToken)
        {
            Dictionary<string, string> queryParams = new()
            {
                ["code"] = request.Code,
                ["state"] = request.State,
            };
            // JACKALOPE: Verify the state properly. 

            string redirectUrl = QueryHelpers.AddQueryString("cherami://", queryParams);

            Log.Error($"Got code {request.Code} and state {request.State}.");
            await Send.RedirectAsync(redirectUrl, true, true);
        }
    }
}