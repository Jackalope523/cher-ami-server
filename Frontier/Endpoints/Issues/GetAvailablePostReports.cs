using FastEndpoints;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using LazyLizardBackend.Shared.Requests;

namespace LazyLizardBackend.Endpoints.Profile
{
    public class GetAvailablePostReports(IReportService reportService) : Endpoint<PostIdRequest, List<PostReportType>>
    {
        public override void Configure()
        {
            Get("issues/posts/{postId}/report");
        }

        public override async Task HandleAsync(PostIdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<PostReportType> response = await reportService.GetAvailableReportsForPostAsync(userId, request.Id);
            await Send.OkAsync(response, cancellationToken);
        }
    }
}
