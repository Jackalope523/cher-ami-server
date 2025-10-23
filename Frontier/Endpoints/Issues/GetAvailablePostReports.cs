using FastEndpoints;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Interfaces.Service;
using CrazyLizard.Entities.Reports;
using CrazyLizard.Shared.Requests;

namespace CrazyLizard.Endpoints.Issues
{
    public class GetAvailablePostReports(IReportService reportService) : Endpoint<IdRequest, List<PostReportType>>
    {
        public override void Configure()
        {
            Get("issues/posts/{id}/report");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<PostReportType> response = await reportService.GetAvailableReportsForPostAsync(userId, request.Id);
            await Send.OkAsync(response, cancellationToken);
        }
    }
}
