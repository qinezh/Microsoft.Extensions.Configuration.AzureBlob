using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Microsoft.Extensions.Configuration.AzureBlob.Core
{
    internal sealed class BlobAccessor : IDisposable
    {
        private int disposed;

        private readonly SemaphoreSlim sync = new SemaphoreSlim(0, 1);

        private readonly Func<CancellationToken, Task<BlobClient>> blobClientFactory;
        private readonly Func<CancellationToken> fetchCancellationTokenFactory;

        public BlobAccessor(
            Func<CancellationToken, Task<BlobClient>> blobClientFactory,
            Func<CancellationToken> fetchCancellationTokenFactory)
        {
            this.blobClientFactory = blobClientFactory ?? throw new ArgumentNullException(nameof(blobClientFactory));
            this.fetchCancellationTokenFactory = fetchCancellationTokenFactory ?? throw new ArgumentNullException(nameof(fetchCancellationTokenFactory));
        }

        public async Task<(ETag, bool)> RetrieveIfUpdated(MemoryStream ms, ETag eTag)
        {
            this.ThrowIfDisposed();

            if (ms == null)
            {
                throw new ArgumentNullException(nameof(ms));
            }

            var ct = this.fetchCancellationTokenFactory();

            var blobClient = await this.blobClientFactory(ct)
                .ConfigureAwait(false);

            var properties = await blobClient.GetPropertiesAsync(new BlobRequestConditions(), ct)
                .ConfigureAwait(false);
            if (properties.Value.ETag == eTag)
            {
                return (properties.Value.ETag, false);
            }

            await blobClient.DownloadToAsync(ms, ct)
                .ConfigureAwait(false);
            return (properties.Value.ETag, true);
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) != 0)
            {
                return;
            }

            sync?.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed == 1)
            {
                throw new ObjectDisposedException(nameof(BlobAccessor));
            }
        }
    }
}
