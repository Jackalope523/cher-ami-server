using CrazyLizard.Entities;
using CrazyLizard.Shared.Responses;
using FastEndpoints;

namespace CrazyLizard.Shared.SharedMappers
{
    public class AccountResponseMapper : ResponseMapper<AccountDTO, User>
    {
        public override AccountDTO FromEntity(User user) => new()
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
