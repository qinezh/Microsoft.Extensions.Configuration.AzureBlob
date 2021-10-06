using System;
using System.Threading;

using Microsoft.Extensions.Configuration.Json;

namespace Microsoft.Extensions.Configuration.AzureBlob.Core
{
    public class BlobJsonConfigurationSource : JsonConfigurationSource
    {
        internal BlobAccessor BlobAccessor { get; set; }
        internal BlobJsonConfigurationOption Option { get; set; }

        public BlobJsonConfigurationSource(BlobJsonConfigurationOption option)
        {
            this.Option = option ?? throw new ArgumentNullException(nameof(option));

            BlobAccessor = new BlobAccessor(
                ct => option.ClientProvider.Get(option.BlobUri, ct),
                () => (option.FetchCancellationTokenFactory ?? (() => CancellationToken.None))());
        }

        public override IConfigurationProvider Build(IConfigurationBuilder builder) => new BlobJsonConfigurationProvider(this);
    }
}
