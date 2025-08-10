using FastEndpoints;
using Frontier.Contracts.Requests;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace LazyLizardBackend.Endpoints.Profile
{
    public class GetAvailableReports(IReportService reportService) : Endpoint<UserIdRequest, List<UserReportType>>
    {
        public override void Configure()
        {
            Get("/account/{userId}/report-types");
        }

        public override async Task HandleAsync(UserIdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await Send.OkAsync(await reportService.GetAvailableReportsForUserAsync(userId, request.Id), cancellationToken);
        }
    }
}
