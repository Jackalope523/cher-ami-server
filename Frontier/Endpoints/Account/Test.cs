using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class Test() : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/test");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            await Send.OkAsync();
        }
    }
}
