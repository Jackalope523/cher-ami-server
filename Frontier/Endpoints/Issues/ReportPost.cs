using FastEndpoints;
using FluentValidation;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace LazyLizardBackend.Endpoints.Posts
{
    public class ReportPostRequest
    {
        public long PostId { get; set; }
        public PostReportType ReportType { get; set; }
        public string ReportDetails { get; set; }
    }

    public class ReportPostRequestValidator : Validator<ReportPostRequest>
    {
        public ReportPostRequestValidator()
        {
            RuleFor(x => x.PostId)
                .GreaterThan(0).WithMessage("PostId must be greater than 0.");

            RuleFor(x => x.ReportType)
                .NotEmpty().WithMessage("Report type is required.");

            RuleFor(x => x.ReportDetails)
                .NotEmpty().WithMessage("Report details are required.")
                .MaximumLength(2000).WithMessage("Report details cannot exceed 2000 characters.");
        }
    }

    public class ReportPost(IReportService reportService) : Endpoint<ReportPostRequest>
    {
        public override void Configure()
        {
            Post("/issues/posts/{postId}/report");
        }

        public override async Task HandleAsync(ReportPostRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await reportService.ReportPostAsync(userId, request.PostId, request.ReportType, request.ReportDetails);
            await Send.NoContentAsync(cancellationToken);
        }
    }
}
