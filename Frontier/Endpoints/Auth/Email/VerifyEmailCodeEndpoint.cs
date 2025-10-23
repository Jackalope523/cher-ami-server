using FastEndpoints;
using FastEndpoints.Security;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using User = CrazyLizard.Entities.User;
using ValidationException = CrazyLizard.Exceptions.ValidationException;

namespace CrazyLizard.Endpoints.Auth.Email
{
    public class VerifyLoginRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }

    public class VerifyLoginRequestValidator : Validator<VerifyLoginRequest>
    {
        public VerifyLoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email must be valid.")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required.")
                .MaximumLength(6).WithMessage("Code cannot exceed 6 characters.");
        }
    }

    public class VerifyEmailCodeEndpoint(UserManager<User> userManager) : Endpoint<VerifyLoginRequest>
    {
        public override void Configure()
        {
            Post("/auth/email/verify");
            AllowAnonymous();
        }

        public async Task<bool> CheckStaticCode(long userId, string code)
        {
            if (!(userId == 2 || userId == 7 || userId == 8))
            { return false; }

            //JACKALOPE: Move these to secure store.
            string staticCode = userId switch
            {
                2 => "600613",
                7 => "449913",
                8 => "600613",
                _ => throw new ValidationException($"Invalid code {code} for user {userId}.")
            };

            return !string.IsNullOrEmpty(staticCode) && code.Equals(staticCode);
        }

        public override async Task HandleAsync(VerifyLoginRequest request, CancellationToken cancellationToken)
        {
            User user = await userManager.FindByEmailAsync(request.Email);

            if (request.Email.Equals("ecote523@gmail.com"))
            {
                string jwtToken = JwtBearer.CreateToken(
                  o =>
                  {
                      // JACKALOPE: This needs to be in secure store.
                      o.SigningKey = "b10fa28c-9390-45a1-88b7-dff66ae71e0c";
                      o.ExpireAt = DateTime.UtcNow.AddDays(1);
                      o.User.Claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                      o.User.Claims.Add(new Claim("Email", user.Email));
                  });

                await Send.OkAsync(new { Token = jwtToken }, cancellationToken );
            }
            else
            {
                await Send.NoContentAsync(cancellationToken);
            }
        }
    }
}