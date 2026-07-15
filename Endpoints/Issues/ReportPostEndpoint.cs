using CherAmiAPI.Services;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Issues
{
    public class ReportPostEndpoint(PostService postService) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Post("/posts/{id}/report");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            long postId = Route<long>("id");

            await postService.ReportPostAsync(userId, postId, cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
