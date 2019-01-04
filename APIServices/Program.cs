namespace APIServices
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;

    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)                   
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((hostingContext, config) => { config.AddJsonFile("appsettings.json", false, true); })
                .UseUrls("http://localhost:808")
                .Build();
    }
}
