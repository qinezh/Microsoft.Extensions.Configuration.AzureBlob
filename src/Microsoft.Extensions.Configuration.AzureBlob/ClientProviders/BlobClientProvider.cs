using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace Microsoft.Extensions.Configuration.AzureBlob.ClientProviders
{
    public abstract class BlobClientProvider
    {
        public abstract Task<BlobClient> Get(Uri blobUri, CancellationToken cancellationToken);
    }
}