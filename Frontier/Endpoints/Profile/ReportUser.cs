using FastEndpoints;
using FluentValidation;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace LazyLizardBackend.Endpoints.Profile
{
    public class ReportUserRequest
    {
        public long UserId { get; set; }

        public UserReportType? ReportType { get; set; }

        public string ReportDetails { get; set; }
    }

    public class ReportUserRequestValidator : Validator<ReportUserRequest>
    {
        public ReportUserRequestValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("UserId must be greater than 0.");

            RuleFor(x => x.ReportType)
                .NotEmpty().WithMessage("Report type is required.");

            RuleFor(x => x.ReportDetails)
                .NotEmpty().WithMessage("Report details are required.")
                .MaximumLength(2000).WithMessage("Report details cannot exceed 2000 characters.");
        }
    }


    public class ReportPost(IReportService reportService) : Endpoint<ReportUserRequest>
    {
        public override void Configure()
        {
            Post("/account/{userId}/report");
        }

        public override async Task HandleAsync(ReportUserRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await reportService.ReportUserAsync(userId, request.UserId, (UserReportType)request.ReportType, request.ReportDetails);
            await Send.NoContentAsync(cancellationToken);
        }
    }
}
