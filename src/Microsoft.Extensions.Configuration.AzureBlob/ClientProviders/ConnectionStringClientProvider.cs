using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Azure.Storage.Blobs;

namespace Microsoft.Extensions.Configuration.AzureBlob.ClientProviders
{
    public class ConnectionStringClientProvider : BlobClientProvider
    {
        private readonly string connectionString;

        public ConnectionStringClientProvider(string connectionString)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public override Task<BlobClient> Get(Uri blobUri, CancellationToken cancellationToken)
        {
            if (blobUri is null)
            {
                throw new ArgumentNullException(nameof(blobUri));
            }

            var container = blobUri.Segments[1].Trim('/');

            // skip first '/' and container name
            var blobName = string.Join(string.Empty, blobUri.Segments.Skip(2));

            var client = new BlobContainerClient(this.connectionString, container).GetBlobClient(blobName);

            return Task.FromResult(client);
        }
    }
}