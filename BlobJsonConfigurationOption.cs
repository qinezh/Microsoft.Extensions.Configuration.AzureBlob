using System;
using System.Linq;

namespace Microsoft.Extensions.Configuration.AzureBlob
{
    public class BlobJsonConfigurationOption
    {
        public Uri BlobUri { get; set; }
        public string SASToken { get; set; }
        public bool IsPublic { get; set; }
        public bool ReloadOnChange { get; set; } = false;
        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);
        public Action<Exception> LogReloadException { get; set; }
        public Action ActionOnReload { get; set; }

        internal static string GetAccount(Uri blobUri)
        {
            if (blobUri == null)
            {
                throw new ArgumentNullException(nameof(blobUri));
            }

            var host = blobUri.Host;
            if (string.IsNullOrEmpty(host))
            {
                return string.Empty;
            }

            return host.Split('.').First();
        }
    }
}
