using Microsoft.Extensions.Configuration.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Configuration.AzureBlob
{
    public class BlobJsonConfigurationSource : JsonConfigurationSource
    {
        internal BlobAccessor BlobAccessor { get; set; }
        internal string ConfigurationFile { get; set; }
        internal TimeSpan PollingInterval { get; set; }

        public BlobJsonConfigurationSource(BlobJsonConfigurationOption option)
        {
            BlobAccessor = new BlobAccessor(option);
            ConfigurationFile = option.ConfigurationFile;
            ReloadOnChange = option.ReloadOnChange;
            PollingInterval = option.PollingInterval;
        }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new BlobJsonConfigurationProvider(this);
        }
    }

    public class BlobJsonConfigurationProvider : JsonConfigurationProvider
    {
        private BlobAccessor _blobAccessor;
        private Timer _timer;
        private readonly TimeSpan _pollingInterval;
        private readonly string _configurationFile;
        private string _etag;

        public BlobJsonConfigurationProvider(BlobJsonConfigurationSource source) : base(source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            _blobAccessor = source.BlobAccessor;
            _configurationFile = source.ConfigurationFile;
            _pollingInterval = source.PollingInterval;

            Load();

            if (source.ReloadOnChange)
            {
                ReloadOnChage();
            }
        }

        public override void Load() => LoadAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        private void ReloadOnChage()
        {
            _timer = new Timer(ReloadOnChange, _blobAccessor, _pollingInterval, _pollingInterval);
        }

        private void ReloadOnChange(object state)
        {
            Load();
        }

        private async Task LoadAsync()
        {
            using (var ms = new MemoryStream())
            {
                var (property, updated) = await _blobAccessor.RetrieveIfUpdated(ms, _configurationFile, _etag);

                if (!updated)
                {
                    return;
                }

                _etag = property.ETag;
                ms.Position = 0;
                base.Load(ms);
            }
        }
    }
}
