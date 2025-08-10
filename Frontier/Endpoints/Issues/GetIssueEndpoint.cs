using FastEndpoints;
using LazyLizardBackend.Shared.Mappers;
using LazyLizardBackend.Shared.Requests;
using LazyLizardBackend.Shared.Responses;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class GetIssueEndpoint(IIssueService issues) : Endpoint<IssueIdRequest, IssueDTO, IssueResponseMapper>
    {
        public override void Configure()
        {
            Get("/issues/{issueId}");
        }

        public override async Task HandleAsync(IssueIdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            CoreIssue response = await issues.GetIssueAsync(userId, request.Id);
            await SendMapped(response, 200, cancellationToken);
        }
    }
}