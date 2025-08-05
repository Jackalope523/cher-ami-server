using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Repository.Contexts;
using System;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class KeyStoreRepository(LLContext ctx) : IKeyRepository
    {
        public Uri Uri = new Uri("https://thesparrowkeys.vault.azure.net/");
        public readonly Func<Azure.Core.TokenCredential> credentials = () => new DefaultAzureCredential();

        private async Task<string> GetSecretAsync(string secretName)
        {
            SecretClient client = new(Uri, credentials());
            KeyVaultSecret secret = await client.GetSecretAsync(secretName);
            return secret.Value;
        }

        public async Task<string> GetHollowOneSignalApiKeyAsync()
        {
            return await GetSecretAsync("OneSignalApiKey");
        }

        public async Task<string> GetHollowOneSignalAppIdAsync()
        {
            return await GetSecretAsync("OneSignalAppId");
        }

        public async Task<string> GetHollowTwilioAccountKeyAsync()
        {
            return await GetSecretAsync("TwilioAccountSID");
        }

        public async Task<string> GetHollowTwilioAuthTokenAsync()
        {
            return await GetSecretAsync("TwilioAuthToken");
        }

        public async Task<string> GetHollowTwilioMessagingServiceAsync()
        {
            return await GetSecretAsync("TwilioMessagingServiceSID");
        }

        public async Task<string> GetAppleAccountCodeAsync()
        {
            return await GetSecretAsync("AppleReviewAccountCode");
        }

        public async Task<string> GetGoogleAccountCodeAsync()
        {
            return await GetSecretAsync("GoogleReviewAccountCode");
        }
    }
}
