using FastEndpoints;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;

namespace LazyLizardBackend.Endpoints.Feedback
{
    public class FeedbackRequest
    {
        public string Comments { get; set; }
        public bool Anonymous { get; set; }

    }

    public class FeedbackRequestValidator : Validator<FeedbackRequest>
    {
        public FeedbackRequestValidator()
        {
            RuleFor(x => x.Comments)
                .NotEmpty().WithMessage("Comments is required.")
                .MaximumLength(300).WithMessage("Comments cannot exceed 300 characters.");
        }
    }


    public class FeedbackEndpoint(IMiscellaneousService miscellaneousService) : Endpoint<FeedbackRequest>
    {
        public override void Configure()
        {
            Post("/feedback");
        }

        public override async Task HandleAsync(FeedbackRequest request, CancellationToken cancellationToken)
        {

            if (request.Anonymous)
            {
                await miscellaneousService.ReceiveFeedback(request.Comments);
                await Send.NoContentAsync(cancellationToken);
            }
            else
            {
                await miscellaneousService.ReceiveFeedback(user.Id, request.Comments);
                await Send.NoContentAsync(cancellationToken);
            }
        }
    }
}
