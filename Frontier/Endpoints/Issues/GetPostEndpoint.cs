using FastEndpoints;
using CrazyLizard.Shared.Mappers;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Shared.Responses;
using CrazyLizard.Interfaces.Service;
using CrazyLizard.Entities;
using CrazyLizard.Shared.Requests;

namespace CrazyLizard.Endpoints.Issues
{
    public class GetPostEndpoint(IIssueService issues) : Endpoint<IdRequest, List<PostDTO>, PostResponseMapper>
    {
        public override void Configure()
        {
            Get("/posts/{id}");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Post corePost = await issues.GetPostAsync(userId, request.Id);
            await SendMapped(corePost, 200, cancellationToken);
        }
    }
}