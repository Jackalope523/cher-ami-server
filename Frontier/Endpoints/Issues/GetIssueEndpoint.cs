using FastEndpoints;
using LazyLizardBackend.Contracts.Requests;
using LazyLizardBackend.Shared.Mappers;
using LazyLizardBackend.Shared.Responses;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class GetIssueEndpoint(IIssueService issues) : Endpoint<IdRequest, IssueDTO, IssueResponseMapper>
    {
        public override void Configure()
        {
            Get("/issues/{id}");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            CoreIssue response = await issues.GetIssueAsync(userId, request.Id);
            await SendMapped(response, 200, cancellationToken);
        }
    }
}