using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration.AzureBlob;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace SampleWebApp
{
    public class Program
    {
        public static ILogger<Program> s_logger;

        public static void Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();
            s_logger = host.Services.GetRequiredService<ILogger<Program>>();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, configuration) =>
                {
                    var settings = configuration.Build();
                    var blobConfig = settings.GetSection("BlobConfiguration");

                    configuration.AddBlobJson(new BlobJsonConfigurationOption
                    {
                        BlobUri = new Uri(blobConfig["BlobUrl"]),
                        IsPublic = true,
                        ReloadOnChange = true,
                        LogReloadException = ex => s_logger.LogError(ex, ex.Message),
                        ActionOnReload = () => s_logger.LogInformation("Reloaded.")
                    });
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
