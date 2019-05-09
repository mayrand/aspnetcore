using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System;
using System.Diagnostics;
using System.IO;
using ThrottlingGateway.Middleware;
using ThrottlingGateway.Models;

namespace ThrottlingGateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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
            logger.LogDebug($"Is64BitProcess: " + Environment.Is64BitProcess);
            app.UseThrottlingMiddleware();
            app.UseHttpLoggingMiddleware();
            app.UseOcelot().Wait();
        }
    }
}
