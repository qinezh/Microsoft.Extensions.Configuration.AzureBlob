using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Extensions.Configuration.AzureBlob
{
    public class BlobAccessor
    {
        protected CloudStorageAccount _storageAccount;
        protected CloudBlobContainer _blobContainer;

        public BlobAccessor(BlobJsonConfigurationOption option)
        {
            _storageAccount = new CloudStorageAccount(option.StorageCredentials, option.StorageAccountName, null, true);
            var cloudBlobClient = _storageAccount.CreateCloudBlobClient();
            _blobContainer = cloudBlobClient.GetContainerReference(option.BlobContainerName);
        }

        public async Task<(BlobProperties, bool)> RetrieveIfUpdated(MemoryStream ms, string blobPath, string eTag)
        {
            var accessCondition = new AccessCondition()
            {
                IfNoneMatchETag = eTag
            };

            var blobRef = _blobContainer.GetBlockBlobReference(blobPath);
            try
            {
                await blobRef.FetchAttributesAsync();
            }
            catch (StorageException ex)
            {
                var result = ex.RequestInformation;
                if (result.HttpStatusCode == 404)
                {
                    return (null, false);
                }
                else
                {
                    throw;
                }
            }

            if (string.Equals(blobRef.Properties.ETag, eTag))
            {
                return (blobRef.Properties, false);
            }

            await blobRef.DownloadToStreamAsync(ms, accessCondition, null, null);
            return (blobRef.Properties, true);
        }
    }
}
