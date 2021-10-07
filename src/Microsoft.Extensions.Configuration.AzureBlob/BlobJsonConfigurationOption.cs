using System;
using System.Threading;

using Microsoft.Extensions.Configuration.AzureBlob.ClientProviders;

namespace Microsoft.Extensions.Configuration.AzureBlob
{
    public class BlobJsonConfigurationOption
    {
        public Uri BlobUri { get; set; }
        public BlobClientProvider ClientProvider { get; set; }
        public Func<CancellationToken> FetchCancellationTokenFactory { get; set; } = () => new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        public TimeSpan? PollingInterval { get; set; } = TimeSpan.FromSeconds(15);
        public Action<Exception> LogReloadException { get; set; }
        public Action ActionOnReload { get; set; }
    }
}
