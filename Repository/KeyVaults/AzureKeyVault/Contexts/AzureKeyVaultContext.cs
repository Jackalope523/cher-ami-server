using Azure.Identity;

namespace Repository
{
    internal class AzureKeyVaultContext
    {
        public Uri Uri = new Uri("https://thesparrowkeys.vault.azure.net/");
        public readonly Func<Azure.Core.TokenCredential> credentials = () => new DefaultAzureCredential();
    }
}
