using FastEndpoints;
using CrazyLizard.Contracts.Requests;
using CrazyLizard.Shared.Mappers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Shared.Responses;
using CrazyLizard.Interfaces.Service;
using CrazyLizard.Entities;

namespace CrazyLizard.Endpoints.Issues
{
    public class GetIssuePostsEndpoint(IIssueService issues) : Endpoint<IdRequest, List<PostDTO>, PostResponseMapper>
    {
        public override void Configure()
        {
            Get("/issues/{id}/posts");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<Post> corePosts = await issues.GetPostsForIssueAsync(userId, request.Id);
            await Send.OkAsync(corePosts.Select(Map.FromEntity).ToList(), cancellationToken);
        }
    }
}