using Azure.Identity;
using static Repository.Harbor;

namespace Repository
{
    public class AzureStorageContext
    {
        public readonly string storageAccountName;
        public readonly string baseUrl = "https://{0}.blob.core.windows.net";
        public readonly Func<Azure.Core.TokenCredential> credentials = () => new DefaultAzureCredential();

        public AzureStorageContext(Flag flag) {
            if (flag == Flag.Production)
            {
                storageAccountName = "canaryproduction";
            }
            else
            {      
                storageAccountName = "sparrowstorageaccount";
            }
        }

        public Uri BuildUri(string containerName)
        {
            return new Uri(string.Format(baseUrl + "/{1}", storageAccountName, containerName));
        }
        public Uri BuildUri(string containerName, string blobName)
        {
            return new Uri(string.Format(baseUrl + "/{1}/{2}", storageAccountName, containerName, blobName));
        }
    }
}
