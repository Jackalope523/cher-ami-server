using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Boundaries;
using Core.Entities;
using Core.Notifications;
using Microsoft.Extensions.Logging;

using static Core.Entities.Arbiter;

namespace Core.Services
{
    public class ProfileService : AbstractService, IProfileOperations
	{
		public ProfileService(CoreTerminal terminal) : base(terminal) { }
        public Task BlockUserAsync(long userId, long targetId)
        {
            throw new NotImplementedException();
        }

        public Task<List<BlockedUserShard>> GetBlockedUsersAsync(long userId)
        {
            throw new NotImplementedException();
        }

        public Task<ProfileShard> GetProfileAsync(long userId, long targetId)
        {
            throw new NotImplementedException();
        }

        public Task UnblockUserAsync(long userId, long targetId)
        {
            throw new NotImplementedException();
        }
    }
}

