using Azure;
using Core.Boundaries;
using FastEndpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mappers
{
    public class UserMapper : ResponseMapper<AccountShard, CoreUser>
    {
        public override AccountShard FromEntity(CoreUser user) => new
        (
            user.Id,
            user.PhoneNumber,
            user.Email,
            user.Title,
            user.GivenName,
            user.FamilyName,
            user.DateOfBirth,
            user.IsPhoneConfirmed,
            user.IsEmailConfirmed,
            user.AccountStatus,
            user.JoinDate,
            user.TimeOfUserAgreement,
            user.NotificationId

        );
    }
}
