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
    public class UserMapper : ResponseMapper<UserShard, CoreUser>
    {
        public override UserShard FromEntity(CoreUser user) => new
        (
            user.Id,
            user.GivenName,
            user.FamilyName
        );
    }
}
