using Core.Boundaries;

namespace Repository
{
    internal class AccountStoreCoordinator : IAccountDatabase
    {
        private readonly IAccountDatabase store;

        public AccountStoreCoordinator(Harbor.Flag flag)
        {
            store = new EFCoreAccountStore(flag);
        }

        public async Task<CoreUser> CreateUserAsync(string phoneNumber, string email, string normalisedEmail, string name, DateTimeOffset dateOfBirth, DateTimeOffset joinDate, CharacterShard character, Guid notificationId)
        {
            return await store.CreateUserAsync(phoneNumber, email, normalisedEmail, name, dateOfBirth, joinDate, character, notificationId);
        }

        public async Task<CoreUser> GetUserByIdAsync(long id) 
        {
            return await store.GetUserByIdAsync(id);
        }

        public async Task<CoreUser> GetUserByPhoneNumberAsync(string phoneNumber) 
        {
            return await store.GetUserByPhoneNumberAsync(phoneNumber);
        }

        public async Task<CoreUser> GetUserByEmailAsync(string email) 
        {
            return await store.GetUserByEmailAsync(email);
        }

        public async Task<HauntShard> GetUserHauntAsync(long id)
        {
            return await store.GetUserHauntAsync(id);
        }

        public async Task<LocationShard> GetRecentLocationAsync(long id)
        {
            return await store.GetRecentLocationAsync(id);
        }    

        public async Task UpdateUserAsync(long id, List<(string Property, object Value)> edits)
        {
            await store.UpdateUserAsync(id, edits);
        }

        public async Task UpdateHauntAsync(long id, double latitude, double longitude, double radius, int stability)
        {
            await store.UpdateHauntAsync(id, latitude, longitude, radius, stability);
        }

        public async Task UpdateRecentLocationAsync(long id, double latitude, double longitude, double radius)
        {
            await store.UpdateRecentLocationAsync(id, latitude, longitude, radius);
        }

        public async Task SoftDeleteAsync(long userId)
        {
            await store.SoftDeleteAsync(userId);
        }

        public async Task HardDeleteAsync(long userId)
        {
            await store.HardDeleteAsync(userId);
        }

        public async Task<bool> UserExistsAsync(string phoneNumber)
        {
            return await store.UserExistsAsync(phoneNumber);
        }

        public async Task<string> RerollUserCodeAsync(long userId)
        {
            return await store.RerollUserCodeAsync(userId);
        }

        public async Task<CoreUser> FindUserByCodeAsync(string code)
        {
            return await store.FindUserByCodeAsync(code);
        }
    }
}
