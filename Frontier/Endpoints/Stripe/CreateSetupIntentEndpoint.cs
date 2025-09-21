using CrazyLizard.Interfaces.Service;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyLizard.Endpoints.Stripe
{
    public record ClientSecretDTO
    {
        public string ClientSecret { get; set; }
    }
    
    public class CreateSetupIntentEndpoint(IAccountService accountService) : Endpoint<CreateSetupIntentEndpoint>
    {
        public override void Configure()
        {
            Post("/stripe/setup-intent");
        }

        public override async Task HandleAsync(CreateSetupIntentEndpoint request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            string clientSecret = await accountService.CreateSetupIntentAsync(userId, cancellationToken);

            ClientSecretDTO response = new()
            {
                ClientSecret = clientSecret,
            };

             await Send.OkAsync(response, cancellationToken);
        }
    }
}