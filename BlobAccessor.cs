using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Extensions.Configuration.AzureBlob
{
    internal class BlobAccessor
    {
        protected CloudBlockBlob _blob;

        public BlobAccessor(Uri blobUri, string account, string sasToken, bool isPublic)
        {
            if (isPublic)
            {
                _blob = new CloudBlockBlob(blobUri);
            }
            else if (string.IsNullOrEmpty(sasToken))
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var tokenAndFrequency = TokenRenewerAsync(azureServiceTokenProvider, CancellationToken.None).GetAwaiter().GetResult();
                var tokenCredential = new TokenCredential(tokenAndFrequency.Token,
                                                            TokenRenewerAsync,
                                                            azureServiceTokenProvider,
                                                            tokenAndFrequency.Frequency.Value);

                var storageCredentials = new StorageCredentials(tokenCredential);
                _blob = new CloudBlockBlob(blobUri, storageCredentials);
            }
            else
            {
                var storageCredentials = new StorageCredentials(account, sasToken);
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

        private static async Task<NewTokenAndFrequency> TokenRenewerAsync(object state, CancellationToken cancellationToken)
        {
            var authResult = await ((AzureServiceTokenProvider)state).GetAuthenticationResultAsync("https://storage.azure.com/");
            var next = (authResult.ExpiresOn - DateTimeOffset.UtcNow) - TimeSpan.FromMinutes(5);
            if (next.Ticks < 0)
            {
                next = default(TimeSpan);
            }

            return new NewTokenAndFrequency(authResult.AccessToken, next);
        }
    }
}
