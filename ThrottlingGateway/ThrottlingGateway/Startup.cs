using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using ThrottlingGateway.Middleware;
using ThrottlingGateway.Models;

namespace ThrottlingGateway
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", false, true);
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOcelot(Configuration);
            services.AddOptions();
            services.Configure<ThrottlingOptions>(Configuration);
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IOptionsMonitor<ThrottlingOptions> optionsAccessor, ILogger<Startup> logger)
        {
            loggerFactory.AddLog4Net();
            logger.LogDebug($"Hosting process model: {Process.GetCurrentProcess().ProcessName}");
            logger.LogDebug($"Current dir: {Directory.GetCurrentDirectory()}");
            app.UseThrottlingMiddleware();
            app.UseHttpLoggingMiddleware();
            app.UseOcelot().Wait();
        }
    }
}
