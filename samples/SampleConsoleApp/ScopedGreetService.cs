using Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace SampleConsoleApp
{
    public class ScopedGreetService
    {
        private readonly ILogger _logger;
        private readonly BlobSetting _options;

        public ScopedGreetService(ILogger<ScopedGreetService> logger, IOptionsSnapshot<BlobSetting> options)
        {
            _options = options.Value;
            _logger = logger;
        }

        public void DoWork(Action<BlobSetting> action)
        {
            action?.Invoke(_options);
        }
    }
}
