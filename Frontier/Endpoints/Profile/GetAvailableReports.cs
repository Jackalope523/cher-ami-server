using FastEndpoints;
using CrazyLizard.Contracts.Requests;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CrazyLizard.Interfaces.Service;

namespace CrazyLizard.Endpoints.Profile
{
    public class GetAvailableReports(IReportService reportService) : Endpoint<IdRequest, List<UserReportType>>
    {
        public override void Configure()
        {
            Get("/account/{id}/report-types");
        }

        public override async Task HandleAsync(IdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await Send.OkAsync(await reportService.GetAvailableReportsForUserAsync(userId, request.Id), cancellationToken);
        }
    }
}
