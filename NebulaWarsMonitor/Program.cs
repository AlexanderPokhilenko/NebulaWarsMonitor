using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NebulaWarsMonitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.Run();
            host.Services.GetRequiredService<ServersCheckerService>().StartAsync(new CancellationToken());
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().
                        UseDefaultServiceProvider(o => o.ValidateScopes = false);
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<ServersCheckerService>();
                });
    }
}