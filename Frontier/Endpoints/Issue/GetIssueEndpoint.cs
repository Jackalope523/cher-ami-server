using FastEndpoints;
using FluentValidation;
using Frontier.Contracts.Responses;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Frontier.Endpoints.Account
{
    public class IssueIdRequest
    {
        public long Id { get; set; }
    }

    public class IssueIdRequestValidator : Validator<IssueIdRequest>
    {
        public IssueIdRequestValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }

    public class AccountResponseMapper : ResponseMapper<AccountDTO, CoreUser>
    {
        public override AccountDTO FromEntity(CoreUser user) => new()
        {
            Id = user.Id,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            Title = user.Title,
            GivenName = user.FirstName,
            FamilyName = user.LastName,
            DateOfBirth = user.DateOfBirth,
            IsPhoneConfirmed = user.IsPhoneConfirmed,
            IsEmailConfirmed = user.IsEmailConfirmed,
            AccountStatus = user.AccountStatus,
            JoinDate = user.JoinDate,
            TimeOfUserAgreement = user.TimeOfUserAgreement,
            NotificationId = user.NotificationId

        };
    }

    public record IssueDTO
    {
        public long Id { get; init; }
        public long CircleId { get; init; }
        public IssueType Type { get; init; }
        public string Title { get; init; }
        public DateTimeOffset StartDate { get; init; }
        public DateTimeOffset EndDate { get; init; }
    }

    public class IssueResponseMapper : ResponseMapper<IssueDTO, CoreIssue>
    {
        public override IssueDTO FromEntity(CoreIssue issue) => new()
        {
            Id = issue.Id,
            CircleId = issue.CircleId,
            Type = issue.Type,
            Title = issue.Title,
            StartDate = issue.StartDate,
            EndDate = issue.EndDate,
        };
    }


    public class GetIssueEndpoint(IIssueService issues) : Endpoint<IssueIdRequest, IssueDTO, IssueResponseMapper>
    {
        public override void Configure()
        {
            Get("/issues/{issueId}");
        }

        public override async Task HandleAsync(IssueIdRequest request, CancellationToken cancellationToken)
        {
            long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            CoreIssue response = await issues.GetIssueAsync(userId, request.Id);
            await SendMapped(response, 200, cancellationToken);
        }
    }
}