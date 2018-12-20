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
        protected CloudBlockBlob _blob;

        public BlobAccessor(Uri blobUri, string account, string accessKey)
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

            _blob = new CloudBlockBlob(blobUri, storageCredentials);
        }

        public async Task<(string, bool)> RetrieveIfUpdated(MemoryStream ms, string eTag)
        {
            if (ms == null)
            {
                throw new ArgumentNullException(nameof(ms));
            }

            try
            {
                await _blob.FetchAttributesAsync();
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

            if (string.IsNullOrEmpty(_blob.Properties?.ETag) && string.Equals(_blob.Properties.ETag, eTag))
            {
                return (_blob.Properties.ETag, false);
            }

            await _blob.DownloadToStreamAsync(ms);
            return (_blob.Properties?.ETag, true);
        }
    }
}
