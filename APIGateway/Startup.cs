namespace APIGateway
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Ocelot.DependencyInjection;
    using Ocelot.Middleware;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class Startup
    {
        private static readonly HttpClient client = new HttpClient();
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(env.ContentRootPath)
                   .AddJsonFile("appsettings.json", false, true)
                   //add configuration.json
                   .AddJsonFile("configuration.json", optional: false, reloadOnChange: true)
                   .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOcelot(Configuration);
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            //console logging
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            app.Use(async (context, next) =>
            {
                if (bool.TryParse(Configuration.GetSection("Delay").Value, out bool useDelay) && useDelay)
                {
                    var method = context.Request.Method;
                    var stringTask = client.GetStringAsync(Configuration.GetSection("Url").Value);
                    if (int.TryParse(await stringTask, out int delay) && delay > 0)
                        await Task.Delay(delay);
                }
                await next();

            });
            app.UseOcelot().Wait();
        }
    }
}
