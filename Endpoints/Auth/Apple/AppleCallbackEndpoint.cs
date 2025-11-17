using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Auth.Apple
{
    public class AppleCallbackRequest
    {
        public string Code { get; set; }
        public string State { get; set; }
    }

    public class AppleCallbackRequestValidator : Validator<AppleCallbackRequest>
    {
        public AppleCallbackRequestValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required.");
        }
    }

    public class AppleCallbackEndpoint() : Endpoint<AppleCallbackRequest>
    {
        public override void Configure()
        {
            Post("/auth/apple/callback");
            AllowFormData(urlEncoded: true);
            AllowAnonymous();
        }

        public override async Task HandleAsync(AppleCallbackRequest request, CancellationToken cancellationToken)
        {
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