using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureBlob;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage.Auth;

namespace SampleWebApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var localConfig = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .Build();

            var blobConfig = localConfig.GetSection("BlobConfiguration");

            var accessToken = new AzureServiceTokenProvider()
                                .GetAccessTokenAsync("https://storage.azure.com/")
                                .ConfigureAwait(false)
                                .GetAwaiter()
                                .GetResult();
            var storageCredentials = new StorageCredentials(new TokenCredential(accessToken));

            Configuration = new ConfigurationBuilder()
                .AddBlobJson(new BlobJsonConfigurationOption
                {
                    StorageAccountName = blobConfig["StorageAccountName"],
                    BlobContainerName = blobConfig["BlobContainerName"],
                    ConfigurationFile = blobConfig["ConfigurationFile"],
                    StorageCredentials = storageCredentials,
                    ReloadOnChange = true
                })
                .Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<BlobSetting>(Configuration);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
