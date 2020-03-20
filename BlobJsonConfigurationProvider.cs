using Microsoft.Extensions.Configuration.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Configuration.AzureBlob
{
    public class BlobJsonConfigurationProvider : JsonConfigurationProvider
    {
        private BlobJsonConfigurationSource _source;
        private Timer _timer;
        private string _etag;
        private int _initialLoad = 0;
        private int _reloadInProgress = 0;

        public BlobJsonConfigurationProvider(BlobJsonConfigurationSource source) : base(source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));

            Load();

            if (_source.Option.ReloadOnChange)
            {
                ReloadOnChage();
            }
        }

        public override void Load()
        {
            try
            {
                LoadAsync().Wait();
            }
            catch (AggregateException ae)
            {
                throw ae.InnerException;
            }
        }

        private void ReloadOnChage()
        {
            _timer = new Timer(ReloadOnChange, null, _source.Option.PollingInterval, _source.Option.PollingInterval);
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
                _source.Option.LogReloadException?.Invoke(ex);
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
                var (etag, updated) = await _source.BlobAccessor.RetrieveIfUpdated(ms, _etag);

                if (!updated)
                {
                    return;
                }

                _etag = etag;
                ms.Position = 0;

                base.Load(ms);

                if (Interlocked.CompareExchange(ref _initialLoad, 1, 0) == 1)
                {
                    _source.Option.ActionOnReload?.Invoke();
                }
            }
        }
    }
}
