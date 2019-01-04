using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SampleConsoleApp
{
    public class CustomizeHostService : IHostedService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly ILogger _logger;
        private Timer _timer;

        public CustomizeHostService(IServiceScopeFactory factory,  ILogger<CustomizeHostService> logger)
        {
            _logger = logger;
            _factory = factory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting...");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping...");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            using (var scope = _factory.CreateScope())
            {
                var scopedService = scope.ServiceProvider.GetRequiredService<ScopedGreetService>();
                scopedService.DoWork(blobSetting => _logger.LogInformation($"team name: {blobSetting.Metadata["team"]}"));
            }
        }
    }
}
