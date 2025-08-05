using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Boundaries;
using LazyLizardBackend;

namespace Core.Services
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

        public async Task<ProfileShard> GetProfileAsync(long userId, long targetId)
        {
            throw new NotImplementedException();
        }

        public async Task UnblockUserAsync(long userId, long targetId)
        {
            await profileRepository.UnblockUserAsync(userId, targetId);
        }
    }
}

