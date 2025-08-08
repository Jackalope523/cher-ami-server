using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Boundaries;
using LazyLizardBackend;

namespace LazyLizardBackend.Services
{
    public class ProfileService(IProfileRepository profileRepository) : IProfileService
	{
        public async Task BlockUserAsync(long userId, long targetId)
        {
            await profileRepository.BlockUserAsync(userId, targetId, Psijic.Time);
        }

        public async Task<List<BlockedUserShard>> GetBlockedUsersAsync(long userId)
        {
            return await profileRepository.GetBlockedUsersAsync(userId);
        }

        public async Task UnblockUserAsync(long userId, long targetId)
        {
            await profileRepository.UnblockUserAsync(userId, targetId);
        }
    }
}

