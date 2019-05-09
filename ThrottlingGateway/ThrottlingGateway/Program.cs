using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace ThrottlingGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls(args.Length < 1 ? "http://localhost:80" : args[0]) // for use with dotnet run only
                .Build();
    }
}
