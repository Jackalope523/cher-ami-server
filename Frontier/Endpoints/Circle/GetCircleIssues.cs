using FastEndpoints;
using CrazyLizard.Contracts.Requests;
using CrazyLizard.Shared.Mappers;
using CrazyLizard.Shared.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Interfaces.Service;

namespace CrazyLizard.Endpoints.Circle
{
    public class getCircleIssues(IIssueService issues) : Endpoint<IdRequest, List<IssueDTO>, IssueResponseMapper>
    {
        public override void Configure()
        {
            Get("/circles/issues/{issueId}");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<CoreIssue> coreIssues = await issues.GetIssuesForCircleAsync(userId, request.Id);
            await Send.OkAsync(coreIssues.Select(Map.FromEntity).ToList(), cancellationToken);
        }
    }
}