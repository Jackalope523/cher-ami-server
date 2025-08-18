using Core.Boundaries;
using FastEndpoints;
using Frontier.Contracts.Responses;

namespace LazyLizardBackend.Shared.SharedMappers
{
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
}
