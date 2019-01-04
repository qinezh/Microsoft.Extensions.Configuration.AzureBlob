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

        public BlobAccessor(Uri blobUri, string account, string accessKey, bool isPublic)
        {
            if (isPublic)
            {
                _blob = new CloudBlockBlob(blobUri);
            }
            else if (string.IsNullOrEmpty(accessKey))
            {
                var accessToken = new AzureServiceTokenProvider()
                    .GetAccessTokenAsync("https://storage.azure.com/")
                    .Result;
                var storageCredentials = new StorageCredentials(new TokenCredential(accessToken));
                _blob = new CloudBlockBlob(blobUri, storageCredentials);
            }
            else
            {
                var storageCredentials = new StorageCredentials(account, accessKey);
                _blob = new CloudBlockBlob(blobUri, storageCredentials);
            }
        }

        public async Task<(string, bool)> RetrieveIfUpdated(MemoryStream ms, string eTag)
        {
            if (ms == null)
            {
                throw new ArgumentNullException(nameof(ms));
            }

            await _blob.FetchAttributesAsync();

            if (string.IsNullOrEmpty(_blob.Properties?.ETag) || string.Equals(_blob.Properties.ETag, eTag))
            {
                return (_blob.Properties.ETag, false);
            }

            await _blob.DownloadToStreamAsync(ms);
            return (_blob.Properties?.ETag, true);
        }
    }
}
