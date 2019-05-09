using log4net;
using log4net.Appender;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ThrottlingGateway.Models;

namespace ThrottlingGateway.Middleware
{
    public class HttpLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _logFile;
        public HttpLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
            _logFile = GetLogFileName("LogFileAppender");
        }

        public async Task InvokeAsync(HttpContext context, ILogger<HttpLoggingMiddleware> logger, IOptionsMonitor<ThrottlingOptions> options)
        {
            if (!options.CurrentValue.LogRequestResponse)
            {
                await _next(context);
            }
            else
            {
                using (var request = new LinkedBuffer())
                using (var response = new LinkedBuffer())
                {
                    (Stream originalResponseBody, string originalRequestBody) = Preprocess(context, response, request);
                    await _next(context);
                    await Postprocess(context, response, originalRequestBody, logger, originalResponseBody);
                }
            }
        }

        private (Stream, string) Preprocess(HttpContext context, LinkedBuffer response, LinkedBuffer request)
        {
            var originalRequestBody = new StreamReader(context.Request.Body).ReadToEnd();
            var bytesToWrite = Encoding.UTF8.GetBytes(originalRequestBody);
            request.Write(bytesToWrite, 0, bytesToWrite.Length);
            request.Seek(0, SeekOrigin.Begin);
            context.Request.Body = request;
            var originalResponseBody = context.Response.Body;
            context.Response.Body = response;
            return (originalResponseBody, originalRequestBody);
        }

        private async Task Postprocess(HttpContext context, LinkedBuffer response, string originalRequestBody, ILogger<HttpLoggingMiddleware> logger, Stream originalResponseBody)
        {
            response.Seek(0, SeekOrigin.Begin);

            logger.LogInformation($"Request.Uri: {context.Request.GetUri()}");
            logger.LogInformation(
                $"Request.Headers:{Environment.NewLine}{GetHeaders(context.Request.Headers)}");

            logger.LogInformation($"Request.Body: {originalRequestBody}");

            logger.LogInformation(
                $"Response.Headers:{Environment.NewLine}{GetHeaders(context.Response.Headers)}");
            logger.LogInformation($"Response.Body: ");
            await LogResponseBody(_logFile, response, originalResponseBody, context);
        }

        private static async Task LogResponseBody(string _logFile, LinkedBuffer response, Stream originalResponseBody, HttpContext context)
        {
            using (var sw = new StreamWriter(_logFile, true))
            using (var sr = new StreamReader(response))
            {
                while (sr.Peek() >= 0)
                {
                    var buffer = new char[Math.Min(response.Length, 1048576)];
                    sr.Read(buffer, 0, buffer.Length);
                    sw.Write(buffer);
                }
                sw.WriteLine();
                response.Seek(0, SeekOrigin.Begin);
                await response.CopyToAsync(originalResponseBody);
                context.Response.Body = originalResponseBody;
            }
        }

        private string GetHeaders(IHeaderDictionary headers)
        {
            var result = new StringBuilder();
            foreach (var header in headers)
            {
                result.AppendLine($"{header.Key}: {header.Value}");
            }
            return result.ToString();
        }
        public static string GetLogFileName(string name)
        {
            var rootAppender = LogManager.GetRepository(Assembly.GetExecutingAssembly())
                .GetAppenders()
                .OfType<FileAppender>()
                .FirstOrDefault(fa => fa.Name == name);

            return rootAppender != null ? rootAppender.File : string.Empty;
        }
    }
}
