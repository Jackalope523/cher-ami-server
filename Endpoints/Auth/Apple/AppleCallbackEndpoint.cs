using CherAmiAPI.Services;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
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

    public class AppleCallbackEndpoint(AuthService authService) : Endpoint<AppleCallbackRequest>
    {
        public override void Configure()
        {
            Post("/auth/apple/callback");
            AllowFormData(urlEncoded: true);
            AllowAnonymous();
        }

        public override async Task HandleAsync(AppleCallbackRequest request, CancellationToken cancellationToken)
        {
            if (request.User != null)
            {
                await authService.SetUserNameByEmailAsync(request.User.Email, request.User.Name.FirstName, request.User.Name.LastName, cancellationToken);
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
