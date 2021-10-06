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
        private readonly string container;

        public ConnectionStringClientProvider(string connectionString, string container)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public override Task<BlobClient> Get(Uri blobUri, CancellationToken cancellationToken)
        {
            // skip first '/' and container name
            var blobName = string.Join(string.Empty, blobUri.Segments.Skip(2));

            var client = new BlobContainerClient(this.connectionString, this.container).GetBlobClient(blobName);

            return Task.FromResult(client);
        }
    }
}