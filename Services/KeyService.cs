using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CherAmiAPI.Interfaces;
using EllipticCurve.Utils;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace CherAmiAPI.Services
{
    public class KeyService(IConfiguration config) : IKeyService
	{
        public Uri Uri = new(config["KEY_VAULT_URI"]);
        public readonly Func<Azure.Core.TokenCredential> credentials = () => new DefaultAzureCredential();

        public async Task<string> GetSecretAsync(string secretName)
        {
            SecretClient client = new(Uri, credentials());
            KeyVaultSecret secret = await client.GetSecretAsync(secretName);
            return secret.Value;
        }
    }
}
