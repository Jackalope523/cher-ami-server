using FastEndpoints;
using LazyLizardBackend.Shared.Requests;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class DeletePostEndpoint(IIssueService issues) : Endpoint<PostIdRequest>
    {
        public override void Configure()
        {
            Delete("/issues/posts/{postId}");
        }

        public override async Task HandleAsync(PostIdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await issues.DeletePostAsync(userId, request.Id);
            await Send.NoContentAsync(cancellationToken);
        }
    }
}