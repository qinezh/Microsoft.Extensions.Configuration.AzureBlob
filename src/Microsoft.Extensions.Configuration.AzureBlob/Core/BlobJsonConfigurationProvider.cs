using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Azure;

using Microsoft.Extensions.Configuration.Json;

namespace Microsoft.Extensions.Configuration.AzureBlob.Core
{
    public class BlobJsonConfigurationProvider : JsonConfigurationProvider
    {
        private BlobJsonConfigurationSource source;
        private Timer timer;
        private ETag etag;
        private int initialLoad = 0;
        private int _reloadInProgress = 0;

        public BlobJsonConfigurationProvider(BlobJsonConfigurationSource source) : base(source)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));

            this.Load();

            this.ReloadOnChange();
        }

        public override void Load()
        {
            LoadAsync().Wait();
        }

        private void ReloadOnChange()
        {
            if (this.source.Option.PollingInterval.HasValue)
            {
                this.timer = new Timer(this.ReloadOnChange, null, this.source.Option.PollingInterval.Value, this.source.Option.PollingInterval.Value);
            }
        }

        private void ReloadOnChange(object _)
        {
            try
            {
                if (Interlocked.CompareExchange(ref _reloadInProgress, 1, 0) == 0)
                {
                    Load();
                }
            }
            catch (Exception ex)
            {
                source.Option.LogReloadException?.Invoke(ex);
            }
            finally
            {
                Interlocked.CompareExchange(ref _reloadInProgress, 0, 1);
            }
        }

        private async Task LoadAsync()
        {
            using (var ms = new MemoryStream())
            {
                var (etagNew, updated) = await source.BlobAccessor.RetrieveIfUpdated(ms, this.etag);

                if (!updated)
                {
                    return;
                }

                this.etag = etagNew;
                ms.Position = 0;

                base.Load(ms);

                if (Interlocked.CompareExchange(ref this.initialLoad, 1, 0) == 1)
                {
                    source.Option.ActionOnReload?.Invoke();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.timer?.Dispose();
                this.timer = null;
            }

            base.Dispose(disposing);
        }
    }
}
