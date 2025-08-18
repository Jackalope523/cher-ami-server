using Core.Boundaries;
using FastEndpoints;
using LazyLizardBackend.Contracts.Requests;
using LazyLizardBackend.Contracts.Responses;
using LazyLizardBackend.Shared.Mappers;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace LazyLizardBackend.Endpoints.Issue
{
    public class GetPostEndpoint(IIssueService issues) : Endpoint<IdRequest, List<PostDTO>, PostResponseMapper>
    {
        public override void Configure()
        {
            Get("/issues/posts/{id}");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            CorePost corePost = await issues.GetPostAsync(userId, request.Id);
            await SendMapped(corePost, 200, cancellationToken);
        }
    }
}