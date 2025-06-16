namespace Repository
{
    public class AzureKeyStore : IKeyDatabase
    {
        AzureKeySentry sentry;

        public AzureKeyStore()
        {
            sentry = new AzureKeySentry();
        }

        public async Task<string> GetHollowOneSignalApiKeyAsync()
        {
            return await sentry.GetSecretAsync("OneSignalApiKey");
        }

        public async Task<string> GetHollowOneSignalAppIdAsync()
        {
            return await sentry.GetSecretAsync("OneSignalAppId");
        }

        public async Task<string> GetHollowTwilioAccountKeyAsync()
        {
            return await sentry.GetSecretAsync("TwilioAccountSID");
        }

        public async Task<string> GetHollowTwilioAuthTokenAsync()
        {
            return await sentry.GetSecretAsync("TwilioAuthToken");
        }

        public async Task<string> GetHollowTwilioMessagingServiceAsync()
        {
            return await sentry.GetSecretAsync("TwilioMessagingServiceSID");
        }

        public async Task<string> GetCanaryMapKeyAsync()
        {
            return await sentry.GetSecretAsync("MapboxSparrowMapToken");
        }

        public async Task<string> GetAppleAccountCodeAsync()
        {
            return await sentry.GetSecretAsync("AppleReviewAccountCode");
        }

        public async Task<string> GetGoogleAccountCodeAsync()
        {
            return await sentry.GetSecretAsync("GoogleReviewAccountCode");
        }
    }
}
