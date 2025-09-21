using FastEndpoints;
using CrazyLizard.Shared.Mappers;
using CrazyLizard.Shared.Responses;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Interfaces.Service;
using CrazyLizard.Entities;

namespace CrazyLizard.Endpoints.Circles
{
    public class GetCurrentIssueEndpoint(IIssueService issues) : EndpointWithoutRequest<IssueDTO, IssueResponseMapper>
    {
        public override void Configure()
        {
            Get("/circle/issues/current");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Issue response = await issues.GetCurrentIssueAsync(userId);
            await SendMapped(response, 200, cancellationToken);
        }
    }
}