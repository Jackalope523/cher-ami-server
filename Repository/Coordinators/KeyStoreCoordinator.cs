using Core.Boundaries;

namespace Repository.Coordinators
{
    public class KeyStoreCoordinator : IKeyDatabase
    {
        IKeyDatabase store;

        public KeyStoreCoordinator()
        {
            store = new AzureKeyStore();
        }

        public async Task<string> GetHollowOneSignalApiKeyAsync()
        {
            return await store.GetHollowOneSignalApiKeyAsync();
        }

        public async Task<string> GetHollowOneSignalAppIdAsync()
        {
            return await store.GetHollowOneSignalAppIdAsync();
        }

        public async Task<string> GetHollowTwilioAccountKeyAsync()
        {
            return await store.GetHollowTwilioAccountKeyAsync();
        }

        public async Task<string> GetHollowTwilioAuthTokenAsync()
        {
            return await store.GetHollowTwilioAuthTokenAsync();
        }

        public async Task<string> GetHollowTwilioMessagingServiceAsync()
        {
            return await store.GetHollowTwilioMessagingServiceAsync();
        }

        public async Task<string> GetCanaryMapKeyAsync()
        {
            return await store.GetCanaryMapKeyAsync();
        }

        public async Task<string> GetAppleAccountCodeAsync()
        {
            return await store.GetAppleAccountCodeAsync();
        }

        public async Task<string> GetGoogleAccountCodeAsync()
        {
            return await store.GetGoogleAccountCodeAsync();
        }
    }
}
