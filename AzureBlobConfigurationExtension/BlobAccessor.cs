using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Extensions.Configuration.AzureBlob
{
    internal class BlobAccessor
    {
        protected CloudBlobContainer _blobContainer;

        public BlobAccessor(string account, string container, string accessKey)
        {
            // Auth by MSI if storage credential is not provided.
            StorageCredentials storageCredentials = null;
            if (string.IsNullOrEmpty(accessKey))
            {
                var accessToken = new AzureServiceTokenProvider()
                    .GetAccessTokenAsync("https://storage.azure.com/")
                    .Result;
                storageCredentials = new StorageCredentials(new TokenCredential(accessToken));
            }
            else
            {
                storageCredentials = new StorageCredentials(account, accessKey);
            }

            var storageAccount = new CloudStorageAccount(storageCredentials, account, null, true);
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            _blobContainer = cloudBlobClient.GetContainerReference(container);
        }

        public async Task<(BlobProperties, bool)> RetrieveIfUpdated(MemoryStream ms, string blobPath, string eTag)
        {
            if (ms == null)
            {
                throw new ArgumentNullException(nameof(ms));
            }

            if (string.IsNullOrEmpty(blobPath))
            {
                throw new ArgumentException($"{nameof(blobPath)} can't be null or empty.");
            }

            var blobRef = _blobContainer.GetBlockBlobReference(blobPath);
            try
            {
                await blobRef.FetchAttributesAsync();
            }
            catch (StorageException ex)
            {
                var requestInfo = ex.RequestInformation;
                if (requestInfo.HttpStatusCode == 404)
                {
                    return (null, false);
                }

                throw;
            }

            if (string.Equals(blobRef.Properties?.ETag, eTag))
            {
                return (blobRef.Properties, false);
            }

            await blobRef.DownloadToStreamAsync(ms);
            return (blobRef.Properties, true);
        }
    }
}
