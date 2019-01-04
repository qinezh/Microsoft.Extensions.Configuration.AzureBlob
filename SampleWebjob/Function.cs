using Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SampleWebjob
{
    public class Function
    {
        private IServiceScopeFactory _factory;

        public Function(IServiceScopeFactory factory)
        {
            _factory = factory;
        }

        public void Run([TimerTrigger("*/5 * * * * *")] TimerInfo timer)
        {
            using (var scope = _factory.CreateScope())
            {
                var processor = scope.ServiceProvider.GetRequiredService<ScopedProcessor>();
                processor.DoWork();
            }
        }
    }

    public class ScopedProcessor
    {
        private BlobSetting _options;
        private ILogger _logger;

        public ScopedProcessor(IOptionsSnapshot<BlobSetting> options, ILogger<ScopedProcessor> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public void DoWork()
        {
            _logger.LogInformation($"team name: {_options.Metadata["team"]}");
        }
    }
}
