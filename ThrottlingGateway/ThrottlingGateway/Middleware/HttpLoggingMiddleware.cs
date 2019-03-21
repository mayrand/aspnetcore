using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThrottlingGateway.Models;

namespace ThrottlingGateway.Middleware
{
    public class HttpLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public HttpLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILogger<HttpLoggingMiddleware> logger, IOptionsMonitor<ThrottlingOptions> options)
        {
            if (!options.CurrentValue.LogRequestResponse)
            {
                await _next(context);
            }
            else
            {
                using (var request = new MemoryStream())
                using (var response = new MemoryStream())
                {
                    (Stream originalResponseBody, string originalRequestBody) = Preproccess(context, response, request);
                    await _next(context);
                    await Postproccess(context, response, originalRequestBody, logger, originalResponseBody);
                }
            }
        }

        private (Stream, string) Preproccess(HttpContext context, MemoryStream response, MemoryStream request)
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

        private async Task Postproccess(HttpContext context, MemoryStream response, string originalRequestBody, ILogger<HttpLoggingMiddleware> logger, Stream originalResponseBody)
        {
            response.Seek(0, SeekOrigin.Begin);

            logger.LogInformation($"Request.Uri: {context.Request.GetUri()}");
            logger.LogInformation(
                $"Request.Headers:{Environment.NewLine}{GetHeaders(context.Request.Headers)}");

            logger.LogInformation($"Request.Body: {originalRequestBody}");

            logger.LogInformation(
                $"Response.Headers:{Environment.NewLine}{GetHeaders(context.Response.Headers)}");
            logger.LogInformation($"Response.Body: {new StreamReader(response).ReadToEnd()}");
            response.Seek(0, SeekOrigin.Begin);

            await response.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;
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
    }
}
