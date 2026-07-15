using CherAmiAPI.Services;
using CherAmiAPI.Shared.Requests;
using FastEndpoints;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CherAmiAPI.Endpoints.Issues
{
    public class DeletePostEndpoint(PostService postService) : Endpoint<IdRequest>
    {
        public override void Configure()
        {
            Delete("/posts/{id}");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await postService.DeletePostAsync(userId, request.Id, cancellationToken);

            await Send.NoContentAsync(cancellationToken);
        }
    }
}
