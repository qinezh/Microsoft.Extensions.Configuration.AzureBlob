using System;
using System.IO;
using System.Threading;
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

        private BlobAccessor(CloudBlockBlob blob)
        {
            _blob = blob;
        }

        public static BlobAccessor Create(Uri blobUri, string account, string sasToken)
        {
            if (!string.IsNullOrEmpty(sasToken))
            {
                var storageCredentials = new StorageCredentials(account, sasToken);
                var blob = new CloudBlockBlob(blobUri, storageCredentials);
                return new BlobAccessor(blob);
            }

            if (IsBlobPublic(blobUri).Result)
            {
                return new BlobAccessor(new CloudBlockBlob(blobUri));
            }

            return CreateBlobAccessorWithAAD(blobUri);
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

        private static async Task<NewTokenAndFrequency> TokenRenewerAsync(object state, CancellationToken cancellationToken)
        {
            var authResult = await ((AzureServiceTokenProvider)state).GetAuthenticationResultAsync("https://storage.azure.com/");
            var next = (authResult.ExpiresOn - DateTimeOffset.UtcNow) - TimeSpan.FromMinutes(5);
            if (next.Ticks < 0)
            {
                next = default;
            }

            return new NewTokenAndFrequency(authResult.AccessToken, next);
        }

        private static async Task<bool> IsBlobPublic(Uri blobUri)
        {
            try
            {
                // check if the blob can be accessed directly.
                await new CloudBlockBlob(blobUri).FetchAttributesAsync();
            }
            catch (StorageException)
            {
                return false;
            }

            return true;
        }

        private static BlobAccessor CreateBlobAccessorWithAAD(Uri blobUri)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var tokenAndFrequency = TokenRenewerAsync(azureServiceTokenProvider, CancellationToken.None).GetAwaiter().GetResult();
            var tokenCredential = new TokenCredential(tokenAndFrequency.Token,
                                                        TokenRenewerAsync,
                                                        azureServiceTokenProvider,
                                                        tokenAndFrequency.Frequency.Value);

            var storageCredentials = new StorageCredentials(tokenCredential);
            var blob = new CloudBlockBlob(blobUri, storageCredentials);
            return new BlobAccessor(blob);
        }
    }
}
