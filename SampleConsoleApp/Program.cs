using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureBlob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System;
using Common;

namespace SampleConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
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
                        LogReloadException = e => Console.WriteLine(e.Message),
                        ActionOnReload = () => Console.WriteLine("Reloaded.")
                    });
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddOptions();
                    services.Configure<BlobSetting>(context.Configuration);
                    services.AddScoped<ScopedGreetService>();
                    services.AddHostedService<CustomizeHostService>();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddConsole();
                });

            await builder.RunConsoleAsync();
        }
    }
}
