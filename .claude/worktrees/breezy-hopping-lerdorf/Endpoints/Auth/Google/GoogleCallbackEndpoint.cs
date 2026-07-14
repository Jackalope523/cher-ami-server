using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Auth.Google
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

            await Send.RedirectAsync(redirectUrl, true, true);
        }
    }
}