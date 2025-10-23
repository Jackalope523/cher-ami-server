using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Interfaces.Service;
using CrazyLizard.Shared.Requests;

namespace CrazyLizard.Endpoints.Issues
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