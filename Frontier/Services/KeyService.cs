using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CherAmiAPI.Interfaces;
using System;
using System.Threading.Tasks;

namespace CrazyLizard.Services
{
    public class KeyService() : IKeyService
	{
        // JACKALOPE: Use config.
        public Uri Uri = new Uri("https://kv-cherami-prod.vault.azure.net/");
        public readonly Func<Azure.Core.TokenCredential> credentials = () => new DefaultAzureCredential();

        public async Task<string> GetSecretAsync(string secretName)
        {
            SecretClient client = new(Uri, credentials());
            KeyVaultSecret secret = await client.GetSecretAsync(secretName);
            return secret.Value;
        }
    }
}
