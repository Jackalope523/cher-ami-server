using Core.Boundaries;
using FastEndpoints;
using CrazyLizard.Contracts.Requests;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class DeletePostEndpoint(IIssueService issues) : Endpoint<IdRequest>
    {
        public override void Configure()
        {
            Delete("/issues/posts/{id}");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await issues.DeletePostAsync(userId, request.Id);
            await Send.NoContentAsync(cancellationToken);
        }
    }
}