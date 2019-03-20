using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Options;
using ThrottlingGateway.Models;

namespace ThrottlingGateway.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ThrottlingMiddleware
    {
        private readonly RequestDelegate _next;
        protected internal ThrottlingOptions _options;
        protected internal ILogger<ThrottlingMiddleware> _logger;
        private static readonly HttpClient client = new HttpClient();
        private LoadInfoResponse previousResponse;

        public ThrottlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, IOptionsMonitor<ThrottlingOptions> options, ILogger<ThrottlingMiddleware> logger)
        {
            _logger = logger;
            _options = options.CurrentValue;
            await Throttle(_options, httpContext.Request.Method);
            await _next(httpContext);
        }

        private async Task Throttle(ThrottlingOptions options, string httpMethodName)
        {
            var methodOptions = options.Methods.FirstOrDefault(o => string.Equals(o.Name, httpMethodName, StringComparison.InvariantCultureIgnoreCase));
            if (methodOptions != null && methodOptions.Enabled)
            {
                _logger.LogInformation($"Throttling enabled for {httpMethodName} method, level: {methodOptions.Level}, function check interval: {options.FunctionLevelCheckInterval}, Prodis busy state url: {options.ThrottlingInfoUrl}, LogRequestResponse: {options.LogRequestResponse}");
                var throttleState = await GetLoadInfoResponse(options.IdleCacheTime, options.BusyCacheTime);
                _logger.LogInformation($"Prodis response - IngestIsProcessing: {throttleState?.IngestIsProcessing}, SmartNodesAreProcessing: {throttleState?.SmartNodesAreProcessing}, TvaIsProcessing: {throttleState?.TvaIsProcessing}.");
                if (IsProdisBusy(throttleState))
                {
                    switch (methodOptions.Level)
                    {
                        case Levels.Low:
                            _logger.LogInformation($"Low throttling for {methodOptions.LowThrottleTime}");
                            await Task.Delay(methodOptions.LowThrottleTime);
                            break;
                        case Levels.High:
                            _logger.LogInformation($"High throttling for {methodOptions.HighThrottleTime}");
                            await Task.Delay(methodOptions.HighThrottleTime);
                            break;
                        case Levels.Function:
                            _logger.LogInformation("Function throttling start.");
                            await WaitUntil(async () =>
                            {
                                throttleState = await GetLoadInfoResponse(options.IdleCacheTime, options.BusyCacheTime);
                                return IsProdisBusy(throttleState);
                            },
                                options.FunctionLevelCheckInterval);
                            _logger.LogInformation("Function throttling end.");
                            break;
                    }
                }
            }
        }

        private async Task WaitUntil(Func<Task<bool>> condition, int frequency = 500, int timeout = -1)
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
                _logger.LogDebug("Returning cached item");
                return previousResponse;
            }
            var loadInfoResponse = await client.GetStringAsync(_options.ThrottlingInfoUrl);
            previousResponse = loadInfoResponse.FromXml();
            return previousResponse;
        }

        private bool CacheItemValid(int idleCacheTime, int busyCacheTime)
        {
            if (previousResponse == null)
            {
                return false;
            }
            if (IsProdisBusy(previousResponse))
            {
                return (DateTime.Now - previousResponse.Occured).TotalMilliseconds < busyCacheTime;
            }
            return (DateTime.Now - previousResponse.Occured).TotalMilliseconds < idleCacheTime;
        }

        private bool IsProdisBusy(LoadInfoResponse throttleState)
        {
            return throttleState != null && (throttleState.IngestIsProcessing ||
                                             throttleState.SmartNodesAreProcessing || throttleState.TvaIsProcessing);
        }
    }
}
