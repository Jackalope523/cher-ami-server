using FastEndpoints;
using Frontier.Contracts.Responses;

namespace Mappers
{
    public class AccountResponseMapper : ResponseMapper<AccountDTO, CoreUser>
    {
        public override AccountDTO FromEntity(CoreUser user) => new()
        { 
            Id = user.Id,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            Title = user.Title,
            GivenName = user.GivenName,
            FamilyName = user.FamilyName,
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
