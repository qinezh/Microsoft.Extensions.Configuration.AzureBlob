using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureBlob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Common;

namespace SampleWebjob
{
    class Program
    {
        static void Main(string[] args)
        {
            ILogger logger = null;
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((context, configuration) =>
                {
                    var localConfig = new ConfigurationBuilder()
                                       .SetBasePath(context.HostingEnvironment.ContentRootPath)
                                       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                       .Build();

                    var blobConfig = localConfig.GetSection("BlobConfiguration");

                    configuration.AddBlobJson(new BlobJsonConfigurationOption
                    {
                        BlobUri = new Uri(blobConfig["BlobUrl"]),
                        ReloadOnChange = true,
                        LogReloadException = ex => logger.LogError(ex, ex.Message),
                        ActionOnReload = () => logger.LogInformation("Reloaded.")
                    });
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddOptions();
                    services.Configure<BlobSetting>(context.Configuration);
                    services.AddScoped<ScopedProcessor>();
                })
                .ConfigureWebJobs(webjobBuilder =>
                {
                    webjobBuilder.AddAzureStorageCoreServices().AddTimers();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddConsole();
                });

            using (var host = builder.Build())
            {
                logger = host.Services.GetRequiredService<ILogger<Program>>();
                host.Run();
            }
        }
    }
}
