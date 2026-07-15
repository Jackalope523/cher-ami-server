using CherAmiAPI.Services;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Issues
{
    public class PostCountEndpoint(PostService postService) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/issue/posts/count");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            int count = await postService.GetLatestIssuePostCountAsync(userId, cancellationToken);

            await Send.OkAsync(count, cancellationToken);
        }
    }
}
