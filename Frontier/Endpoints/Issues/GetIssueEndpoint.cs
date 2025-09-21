using FastEndpoints;
using CrazyLizard.Contracts.Requests;
using CrazyLizard.Shared.Mappers;
using CrazyLizard.Shared.Responses;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Interfaces.Service;
using CrazyLizard.Entities;

namespace CrazyLizard.Endpoints.Issues
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
            Issue response = await issues.GetIssueAsync(userId, request.Id);
            await SendMapped(response, 200, cancellationToken);
        }
    }
}