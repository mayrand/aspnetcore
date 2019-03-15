using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using ThrottlingGateway.Models;

namespace ThrottlingGateway
{
    public class Startup
    {
        private static readonly HttpClient client = new HttpClient();
        private ILogger<Startup> logger;
        private LoadInfoResponse previousResponse;
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile("configuration.json", optional: false, reloadOnChange: true);
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
            this.logger = logger;
            loggerFactory.AddLog4Net();
            logger.LogDebug($"Hosting process model: {Process.GetCurrentProcess().ProcessName}");
            logger.LogDebug($"Current dir: {Directory.GetCurrentDirectory()}");
            app.Use(async (context, next) =>
            {
                await Throttle(optionsAccessor.CurrentValue, context.Request.Method);
                await next();
            });
            app.UseOcelot().Wait();
        }

        private async Task Throttle(ThrottlingOptions options, string httpMethodName)
        {
            var methodOptions = options.Methods.FirstOrDefault(o => string.Equals(o.Name, httpMethodName, StringComparison.InvariantCultureIgnoreCase));
            if (methodOptions != null && methodOptions.Enabled)
            {
                logger.LogInformation($"Throttling enabled for {httpMethodName} method, level: {methodOptions.Level}, function check interval: {options.FunctionLevelCheckInterval}, Prodis busy state url: {options.ThrottlingInfoUrl}");
                var throttleState = await GetLoadInfoResponse(options.IdleCacheTime, options.BusyCacheTime);
                logger.LogInformation($"Prodis response - IngestIsProcessing: {throttleState?.IngestIsProcessing}, SmartNodesAreProcessing: {throttleState?.SmartNodesAreProcessing}, TvaIsProcessing: {throttleState?.TvaIsProcessing}.");
                if (ProdisIsBusy(throttleState))
                {
                    switch (methodOptions.Level)
                    {
                        case "low":
                            logger.LogInformation($"Low throttling for {methodOptions.LowThrottleTime}");
                            await Task.Delay(methodOptions.LowThrottleTime);
                            break;
                        case "high":
                            logger.LogInformation($"High throttling for {methodOptions.HighThrottleTime}");
                            await Task.Delay(methodOptions.HighThrottleTime);
                            break;
                        case "function":
                            logger.LogInformation($"Function throttling start.");
                            await WaitUntil(async () =>
                            {
                                throttleState = await GetLoadInfoResponse(options.IdleCacheTime, options.BusyCacheTime);
                                return ProdisIsBusy(throttleState);
                            },
                                options.FunctionLevelCheckInterval);
                            logger.LogInformation($"Function throttling end.");
                            break;
                    }
                }
            }
        }

        public static async Task WaitUntil(Func<Task<bool>> condition, int frequency = 500, int timeout = -1)
        {
            var waitTask = Task.Run(async () =>
            {
                while (await condition()) await Task.Delay(frequency);
            });

            if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout)))
            {
                throw new TimeoutException();
            }
        }

        private async Task<LoadInfoResponse> GetLoadInfoResponse(int idleCacheTime, int busyCacheTime)
        {
            if (CacheItemValid(idleCacheTime, busyCacheTime))
            {
                logger.LogDebug("Returning cached item");
                return previousResponse;
            }
            var loadInfoResponse = await client.GetStringAsync(Configuration.GetSection("ThrottlingInfoUrl").Value);
            previousResponse = FromXml(loadInfoResponse);
            return previousResponse;
        }

        private bool CacheItemValid(int idleCacheTime, int busyCacheTime)
        {
            if (previousResponse == null)
            {
                return false;
            }
            if (ProdisIsBusy(previousResponse))
            {
                return (DateTime.Now - previousResponse.Occured).TotalSeconds < busyCacheTime;
            }
            return (DateTime.Now - previousResponse.Occured).TotalSeconds < idleCacheTime;
        }

        private LoadInfoResponse FromXml(string Xml)
        {
            var ser = new XmlSerializer(typeof(LoadInfoResponse));
            var stringReader = new StringReader(Xml);
            var xmlReader = new XmlTextReader(stringReader);
            var obj = ser.Deserialize(xmlReader) as LoadInfoResponse;
            obj.Occured = DateTime.Now;
            xmlReader.Close();
            stringReader.Close();
            return obj;
        }

        private bool ProdisIsBusy(LoadInfoResponse throttleState)
        {
            return throttleState != null && (throttleState.IngestIsProcessing ||
                                             throttleState.SmartNodesAreProcessing || throttleState.TvaIsProcessing);
        }
    }
}
