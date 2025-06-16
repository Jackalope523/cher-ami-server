using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;


namespace Repository
{
    internal class AzureKeySentry
    {
        AzureKeyVaultContext context = new();

        public async Task<string> GetSecretAsync(string secretName)
        {
            try
            {
                SecretClient client = new(context.Uri, context.credentials());
                KeyVaultSecret secret = await client.GetSecretAsync(secretName);
                return secret.Value;
            }
            catch (Exception ex)
            {
                throw new VaultIOException(ex);
            }    
        }

        public async Task<object> GetKeyAsync(string keyName)
        {
            try
            {
                KeyClient client = new(context.Uri, context.credentials());
                KeyVaultKey key = await client.GetKeyAsync(keyName);
                return key.Key;
            }
            catch (Exception ex)
            {
                throw new VaultIOException(ex);
            }
        }

        public async Task<byte[]> GetCertificateAsync(string certificateName)
        {
            try
            {
                CertificateClient client = new(context.Uri, context.credentials());
                KeyVaultCertificate certificate = await client.GetCertificateAsync(certificateName);
                return certificate.Cer;
            }
            catch (Exception ex)
            {
                throw new VaultIOException(ex);
            }
        }
    }
}
