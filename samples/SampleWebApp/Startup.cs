using Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureBlob;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SampleWebApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var localConfig = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional:false, reloadOnChange: true)
                .Build();

            var blobConfig = localConfig.GetSection("BlobConfiguration");

            Configuration = new ConfigurationBuilder()
                .AddBlobJson(new BlobJsonConfigurationOption
                {
                    BlobUri = new Uri(blobConfig["BlobUrl"]),
                    ReloadOnChange = true,
                    LogReloadException = e => Console.WriteLine(e.Message),
                    ActionOnReload = () => Console.WriteLine("Reloaded.")
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

            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
